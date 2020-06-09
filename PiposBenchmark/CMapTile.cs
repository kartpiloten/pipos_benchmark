using Npgsql;
using NpgsqlTypes;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using Npgsql.NetTopologySuite;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PiposBenchmark
{
    [Serializable]
    public class CMapTileList
    {
        public List<CMapTile> theMapList { get; set; }

        public CMapTileList()
        {
            theMapList = new List<CMapTile>();
        }
        public CMapTileList(CStartParameters aStartParameters)
        {
            Int64 total_number_tiles = (aStartParameters.x_max - aStartParameters.x_min) / 250 * (aStartParameters.y_max - aStartParameters.y_min) / 250;
            Int64 number_of_tiles_created = 0;
            theMapList = new List<CMapTile>();
            //            for (Int64 x = 236000; x < 6076000;x=x+250)
            for (Int64 x = aStartParameters.x_min; x < aStartParameters.x_max; x = x + 250)
            {
                for (Int64 y = aStartParameters.y_min; y < aStartParameters.y_max; y = y + 250)
                {
                    CMapTile aChapTile = new CMapTile(x,y);
                    theMapList.Add(aChapTile);
                    
                    number_of_tiles_created++;
                    if (number_of_tiles_created % 100 == 0 && number_of_tiles_created != 0)
                    {
                        Console.Write("\r{0}   {1}", number_of_tiles_created, " The target number is " + total_number_tiles);
                    }
                    
                }
            }
            Console.WriteLine("\r "+ total_number_tiles  + " tiles were created                                     ");
        }

        public static void writeListToFileToDiskBinary(string pathToFile, CMapTileList aMapTileList) 
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(pathToFile, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, aMapTileList);
            stream.Close();
        }

        public static void readListToFileToDiskBinary(string pathToFile, CStartParameters aStartParameters)
        {
            CMapTileList aMapaMapTileList = null;
            FileStream fs = new FileStream(pathToFile, FileMode.Open);
            IFormatter formatter = new BinaryFormatter();
            aMapaMapTileList = (CMapTileList)formatter.Deserialize(fs);
            fs.Close();
            if (aStartParameters.removeTestFile_DatabaseAfterTest)
            {
                File.Delete(pathToFile);
            }

        }

        public void writeListToDatabase(string connectionstring)
        {
            using (var conn = new NpgsqlConnection(connectionstring))
            {
                conn.Open();
                //System.Threading.Thread.Sleep(1000); // Crashes on next line?? 
                conn.TypeMapper.UseNetTopologySuite();

                string CommandText = @"DROP TABLE IF EXISTS public.tiletest; CREATE TABLE public.tiletest(id_tile_250 bigint, the_geom geometry); ";
                NpgsqlCommand createtbl_cmd = new NpgsqlCommand(CommandText, conn);
                createtbl_cmd.Connection = conn;
                createtbl_cmd.ExecuteNonQuery();

                // note that it is overkill to do bulk import for two objects, but as example... 
                using (var writer = conn.BeginBinaryImport("COPY public.tiletest(id_tile_250, the_geom ) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var entity in this.theMapList)
                    {
                        // Note the StartRow and Complete
                        // Geometry types from NetTopologySuite.Geometries
                        Coordinate[] aCoordinateArray = new Coordinate[5];
                        aCoordinateArray[0] = entity.lowerEastCorner;
                        aCoordinateArray[1] = entity.lowerWestCorner;
                        aCoordinateArray[2] = entity.upperWestCorner;
                        aCoordinateArray[3] = entity.upperEastCorner;
                        aCoordinateArray[4] = entity.lowerEastCorner;
                        LinearRing aLinearring = new LinearRing(aCoordinateArray);
                        Polygon tile_geom = new Polygon(aLinearring);
                        tile_geom.SRID = 3006;
                        writer.StartRow();
                        writer.Write(entity.TileID, NpgsqlDbType.Bigint);
                        writer.Write(tile_geom, NpgsqlDbType.Geometry);
                    }
                    writer.Complete();
                }
                conn.Close();
                
            }
        }
        public void readListFromDatabase(string connectionstring)
        {
            using (var conn = new NpgsqlConnection(connectionstring))
            {
                conn.Open(); 
                conn.TypeMapper.UseNetTopologySuite();
                CMapTileList aMapTileList = new CMapTileList();

                // note that it is overkill to do bulk import for two objects, but as example... 
                using (var reader = conn.BeginBinaryExport("COPY public.tiletest(id_tile_250, the_geom ) TO STDOUT (FORMAT BINARY)"))
                {
                    while (reader.StartRow() > 0)
                    {
                        long aMapId = reader.Read<long>(NpgsqlDbType.Bigint);
                        Polygon aPolygon = reader.Read<Polygon>(NpgsqlDbType.Geometry);
                        Coordinate aCoordinate = aPolygon.Coordinates[0];
                        CMapTile aCmapTile = new CMapTile((long)aCoordinate.X, (long)aCoordinate.Y);
                        aCmapTile.TileID = aMapId;
                        aMapTileList.theMapList.Add(aCmapTile);
                    }
                    reader.Cancel();
                }
                conn.Close();
            }

        }

        public static void  GetSizeOfTable(string ConnString)
        {
            string result = "";
            NpgsqlConnection conn = new NpgsqlConnection(ConnString);
            conn.Open();

            NpgsqlCommand command = new NpgsqlCommand(
                "SELECT pg_size_pretty(  pg_total_relation_size('public.tiletest')  );", conn);

            NpgsqlDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                result = Convert.ToString(dr[0]);
                Console.WriteLine("The size of the databasetable is: " + result);              
            }
            conn.Close();

        }
    }
    [Serializable]
    public class CMapTile
    {
        public Int64 TileID;
        public Coordinate lowerWestCorner;
        public Coordinate lowerEastCorner;
        public Coordinate upperEastCorner;
        public Coordinate upperWestCorner;

        public CMapTile(Int64 x, Int64 y)
        {
            TileID = Convert.ToInt64(x.ToString() + y.ToString());
            lowerWestCorner = new Coordinate(x,y);
            lowerEastCorner = new Coordinate(x, y+250);
            upperEastCorner = new Coordinate(x + 250, y);
            upperWestCorner = new Coordinate(x + 250, y + 250);
        }
    }
}
   