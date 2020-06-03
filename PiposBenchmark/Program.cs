
using System;
using System.Diagnostics;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore.Metadata;

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
            CMapTileList aMaptileList = new CMapTileList();
            sw.Stop();
            Console.WriteLine();
            Console.WriteLine("It took {0} to create testdata", sw.Elapsed);
            sw.Reset();


            //Test disk performace
            // 10 possible test slots
            Console.WriteLine("Test performace of diskconnection.");
            for (int i =0;i<10;i++)
            {
                if (aStartParColl.targetFolders[i] != null)
                {
                    sw.Start();
                    aMaptileList.writeListToFileToDisk(aStartParColl.targetFolders[i]);
                    //aMaptileList.writeListToFileToDisk(@"D:\temp\test.txt");
                    sw.Stop();
                    Console.WriteLine("{0}   Timemeasure={1}", aStartParColl.targetFoldersName[i], sw.Elapsed);
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
                    aMaptileList.writeListToDatabase(aStartParColl.connectStrings[0]);
                    sw.Stop();

                    Console.WriteLine("{0}   Timemeasure={1}", aStartParColl.connectStringsName[i], sw.Elapsed);
                    sw.Reset();
                }
            }
            Console.WriteLine("Done! Press any key to close.");
            Console.ReadKey();
        }
    }
}
