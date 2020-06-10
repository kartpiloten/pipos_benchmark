
using System;
using System.Diagnostics;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IO;

namespace PiposBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();

            // Read start parameter file
            CStartParameters aStartParColl = new CStartParameters("StartParamsPiposBenchmark.txt");

            //construct testdata  
            Console.WriteLine("Starts performance test");
            Console.WriteLine("Creates testtiles");
            sw.Start();
            CMapTileList aMaptileList = new CMapTileList(aStartParColl);
            sw.Stop();
            Console.WriteLine("It took {0} to create testdata", sw.Elapsed);
            Console.WriteLine();
            sw.Reset();


            //Test disk performace
            // 10 possible test slots
            Console.WriteLine();
            Console.WriteLine("Test performace of diskconnection.");
            for (int i =0;i<10;i++)
            {
                if (aStartParColl.targetFolders[i] != null)
                {
                    CMapTileList.writeListToFileToDiskBinary(sw, aStartParColl.targetFolders[i], aMaptileList);
                    Console.WriteLine("{0}   Write Timemeasure={1}", aStartParColl.targetFoldersName[i], sw.Elapsed);
                    sw.Reset();
                    FileInfo fi = new FileInfo(aStartParColl.targetFolders[i]);
                    Console.WriteLine("File Size in Megabytes: {0}", ((Int64)(fi.Length / 1024f) / 1024f));
                    CMapTileList.readListToFileToDiskBinary(sw, aStartParColl.targetFolders[i], aStartParColl);
                    Console.WriteLine("{0}   Read Timemeasure={1}", aStartParColl.targetFoldersName[i], sw.Elapsed);
                    Console.WriteLine();
                    sw.Reset();
                }
            }
            //Test database performace
            // 10 possible test slots
            Console.WriteLine("Test performace of databaseconnection.");
            for (int i = 0; i < 10; i++)
            {
                if (aStartParColl.connectStrings[i] != null)
                {
                    sw.Start();
                    aMaptileList.writeListToDatabase(aStartParColl.connectStrings[i]);
                    sw.Stop();
                    Console.WriteLine("{0}   Write Timemeasure={1}", aStartParColl.connectStringsName[i], sw.Elapsed);
                    sw.Reset();
                    CMapTileList.GetSizeOfTable(aStartParColl.connectStrings[i]);
                    sw.Start();
                    aMaptileList.readListFromDatabase(aStartParColl.connectStrings[i]);
                    sw.Stop();
                    Console.WriteLine("{0}   Read Timemeasure={1}", aStartParColl.connectStringsName[i], sw.Elapsed);
                    Console.WriteLine();
                    sw.Reset();
                }
            }
            Console.WriteLine();
            Console.WriteLine("Done! Press any key to close.");
            Console.ReadKey();
        }
    }
}
