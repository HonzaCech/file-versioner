using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace FileVersioningTool.FileVersioningBackend
{
    public class FileVersioner : IFileVersioner
    {
        private readonly IFileVersioner _fileVersioner;

        public FileVersioner(IFileVersioner fileVersioner)
        {
            _fileVersioner = fileVersioner;
        }

        public IList<VersionedFile> GetChangesList(string basePath)
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

            return null;


        }
    }
}
