using Npgsql;
using NpgsqlTypes;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using Npgsql.NetTopologySuite;
using System.Linq.Expressions;

namespace PiposBenchmark
{
    public class CMapTileList
    {
        public List<CMapTile> theMapList { get; set; }
        public CMapTileList()
        {
            Int64 number_of_tiles_created = 0;
            theMapList = new List<CMapTile>();
            //            for (Int64 x = 236000; x < 6076000;x=x+250)
            for (Int64 x = 236000; x < 270000; x = x + 250)
            {
                for (Int64 y = 991000; y < 7697000; y = y + 250)
                {
                    CMapTile aChapTile = new CMapTile(x,y);
                    theMapList.Add(aChapTile);
                    number_of_tiles_created++;
                    if (number_of_tiles_created % 100 == 0 && number_of_tiles_created != 0)
                    {
                        Console.Write("\r{0}   {1}", number_of_tiles_created, " The target number is 3648064");
                    }
                    
                }
            }
            Console.Write("\r{0}   {1}", number_of_tiles_created, " The target number is 3648064");
        }
        //public static void writeListToFileToDisk(CMapTileList aMapTileList, string pathToFile)
        public void writeListToFileToDisk(string pathToFile)
        {
            try
            {
                if (File.Exists(pathToFile))
                {
                    File.Delete(pathToFile);
                }
                using (StreamWriter fs = File.CreateText(pathToFile))
                {
                    foreach (var entity in this.theMapList)
                    {
                        fs.WriteLine(entity.TileID + " ; (POLYGON((" + entity.lowerWestCorner.ToString() + ", " + entity.lowerEastCorner.ToString() + ", " + entity.upperEastCorner.ToString() + ", " + entity.upperWestCorner.ToString() + ", " + entity.lowerWestCorner.ToString()) ;    // + " , " 10 20, 20 40, 40 40, 30 10)));
                    }
                    fs.Close();
                    //File.Delete(pathToFile);
                }
            }
            catch
            {

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
 //               string CommandText = @"DROP TABLE IF EXISTS public.tiletest; ";
 //               NpgsqlCommand createtbl_cmd = new NpgsqlCommand(CommandText, conn);
 //               createtbl_cmd.Connection = conn;
 //               createtbl_cmd.ExecuteNonQuery();
            }

        }
    }

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
   