using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace PiposBenchmark
{
    public class CStartParameters
    {
        public Int64 x_min;
        public Int64 x_max;
        public Int64 y_min;
        public Int64 y_max;

        public bool removeTestFile_DatabaseAfterTest;

        public string[] targetFoldersName = new string[10];
        public string[] targetFolders = new string[10];
        public string[] connectStringsName = new string[10];
        public string[] connectStrings = new string[10];

        public CStartParameters(string pathConfigFile)
        {
            var fileStream = new FileStream(pathConfigFile, FileMode.Open, FileAccess.Read);
            var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                if (line == "..areaOfTiles")
                {
                    line = streamReader.ReadLine();
                    string[] lineArray = line.Split("..");
                    x_min = Convert.ToInt64(lineArray[0]);
                    x_max = Convert.ToInt64(lineArray[1]);
                    y_min = Convert.ToInt64(lineArray[2]);
                    y_max = Convert.ToInt64(lineArray[3]);
                }
                else if (line == "..removeTestFile_DatabaseAfterTest")
                {
                    line = streamReader.ReadLine();
                    if (line == "yes")
                    {
                        this.removeTestFile_DatabaseAfterTest = true;
                    }
                }
                else if (line == ".. Target Folders")
                {
                    int i = 0;
                    while ((line = streamReader.ReadLine()) != "..End of input(Line must stay as is)")
                    {
                        string[] lineArray = line.Split("..");
                        this.targetFoldersName[i] = lineArray[0];
                        this.targetFolders[i] = lineArray[1];
                        i++;
                    }
                }
                else if (line == ".. Connections string")
                {
                    int i = 0;
                    while ((line = streamReader.ReadLine()) != "..End of input(Line must stay as is)")
                    {
                        string[] lineArray = line.Split("..");
                        this.connectStringsName[i] = lineArray[0];
                        this.connectStrings[i] = lineArray[1];
                        i++;
                    }
                }
            }
        }
    }
}
