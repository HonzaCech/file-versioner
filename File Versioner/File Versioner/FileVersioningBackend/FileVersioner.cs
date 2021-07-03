using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace FileVersioningTool.FileVersioningBackend
{
    public class FileVersioner
    {
        public void GetChangesList(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                throw new ArgumentException($"Directory {basePath} does not exist");
            }

            var versioningFilePath = Path.Combine(basePath, ".versions");            
            if (!File.Exists(versioningFilePath))
            {

            }
            else
            {

            }



        }
    }
}
