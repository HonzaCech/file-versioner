using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using FileVersioningTool.Models;

namespace FileVersioningTool.FileVersioningBackend
{
    public class FileVersioner : IFileVersioner
    {

        public async Task<IList<VersionedFile>> GetChangesListAsync(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                throw new ArgumentException($"Directory {basePath} does not exist");
            }

            var versioningFilePath = Path.Combine(basePath, ".versions");
            if (!File.Exists(versioningFilePath))
            {

                return processNewDir(basePath, versioningFilePath);
            }
            else
            {
                using var md5 = MD5.Create();
                using FileStream openStream = File.OpenRead(versioningFilePath);
                var originalFiles = await JsonSerializer.DeserializeAsync<List<VersionedFile>>(openStream);
                openStream.Dispose();
                var originalFilesSet = new HashSet<VersionedFile>(originalFiles);

                string[] allActualFiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);
                var actualFilesSet = new HashSet<VersionedFile>(allActualFiles.Select(filename => new VersionedFile() { Name = Path.GetRelativePath(basePath, filename) }));
                actualFilesSet.Remove(new VersionedFile() { Name = ".versions" });

                var possiblyModifiedFiles = new HashSet<VersionedFile>(originalFilesSet);
                possiblyModifiedFiles.IntersectWith(actualFilesSet);

                var deletedFiles = new HashSet<VersionedFile>(originalFilesSet);
                deletedFiles.ExceptWith(actualFilesSet);

                var addedFiles = new HashSet<VersionedFile>(actualFilesSet);
                addedFiles.ExceptWith(originalFilesSet);

                processFileSets(basePath, md5, possiblyModifiedFiles, deletedFiles, addedFiles);

                var allFiles = new List<VersionedFile>();
                allFiles.AddRange(deletedFiles);
                allFiles.AddRange(addedFiles);
                allFiles.AddRange(possiblyModifiedFiles);

                var options = new JsonSerializerOptions { WriteIndented = true };
                using FileStream createStream = File.Create(versioningFilePath);
                JsonSerializer.SerializeAsync(createStream, allFiles.Where(f => f.Status != FileStatus.DELETED), options);
                return allFiles.Where(f => f.Status != FileStatus.UNCHANGED).ToList();
            }
        }

        private static void processFileSets(string basePath, MD5 md5, HashSet<VersionedFile> possiblyModifiedFiles, HashSet<VersionedFile> deletedFiles, HashSet<VersionedFile> addedFiles)
        {
            foreach (var file in deletedFiles)
            {
                file.Status = FileStatus.DELETED;
            }

            foreach (var file in addedFiles)
            {
                var hash = getHash(md5, Path.Combine(basePath, file.Name));
                file.Hash = hash;
                file.Version = 1;
                file.Status = FileStatus.ADDED;
            }

            checkPossibleModifications(basePath, md5, possiblyModifiedFiles);
        }

        private static void checkPossibleModifications(string basePath, MD5 md5, HashSet<VersionedFile> possiblyModifiedFiles)
        {
            foreach (var file in possiblyModifiedFiles)
            {

                var hash = getHash(md5, Path.Combine(basePath, file.Name));
                if (hash != file.Hash)
                {
                    file.Version += 1;
                    file.Hash = hash;
                    file.Status = FileStatus.MODIFIED;
                }
                else
                {
                    file.Status = FileStatus.UNCHANGED;
                }
            }
        }

        private static IList<VersionedFile> processNewDir(string basePath, string versioningFilePath)
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
            using var fileStream = File.Create(versioningFilePath);            
            JsonSerializer.SerializeAsync(fileStream, processedFiles, options);            
            return processedFiles;
        }

        private static string getHash(MD5 md5, string filename)
        {
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
