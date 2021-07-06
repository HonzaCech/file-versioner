using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace FileVersioningTool.FileVersioningBackend
{
    public class FileVersioner : IFileVersioner
    {

        public IList<VersionedFile> GetChangesList(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                throw new ArgumentException($"Directory {basePath} does not exist");
            }

            var versioningFilePath = Path.Combine(basePath, ".versions");            
            if (!File.Exists(versioningFilePath))
            {
                var processedFiles = new List<VersionedFile>();
                string[] allFiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
                using var md5 = MD5.Create();
                foreach (var fullFilePath in allFiles)
                {
                    var hash = getHash(md5, fullFilePath);
                    var versionedFile = new VersionedFile() { Name = Path.GetRelativePath(basePath, fullFilePath), Hash = hash, Status = FileStatus.ADDED, Version = 1 };
                    processedFiles.Add(versionedFile);
                }

                
                var options = new JsonSerializerOptions { WriteIndented = true };
                using FileStream createStream = File.Create(versioningFilePath);
                JsonSerializer.SerializeAsync(createStream, processedFiles, options);
                createStream.DisposeAsync();
            }
            else
            {
                using FileStream openStream = File.OpenRead(versioningFilePath);
                var originalFiles = await JsonSerializer.DeserializeAsync<List<VersionedFile>>(openStream);
                var originalFilesSet = new HashSet<VersionedFile>(originalFiles);

            }

            return null;


        }

        private static string getHash(MD5 md5, string filename)
        {
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
