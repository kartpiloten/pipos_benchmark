using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace PiposBenchmark
{
    public class CStartParameters
    {
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
                if (line == ".. Target Folders")
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
                if (line == ".. Connections string")
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
