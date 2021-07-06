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
        private static readonly MD5 md5 = MD5.Create();

        /// <summary>
        /// Gets a list of all files that were changed in a given path (recursively) from the previous run, or of all files if first run on this path
        /// </summary>
        /// <param name="basePath">Path to process. Must be a valid directory path</param>
        /// <returns>List of all modified files</returns>
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
                //Don't wanna version the versioning file
                actualFilesSet.Remove(new VersionedFile() { Name = ".versions" });

                HashSet<VersionedFile> possiblyModifiedFiles = processPossibleModifications(basePath, originalFilesSet, actualFilesSet);

                HashSet<VersionedFile> deletedFiles = processDeletedFiles(originalFilesSet, actualFilesSet);

                HashSet<VersionedFile> addedFiles = processAddedFiles(basePath, originalFilesSet, actualFilesSet);

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

        /// <summary>
        /// Gets a set of all files that were added
        /// </summary>
        /// <param name="basePath">Directory to process</param>
        /// <param name="originalFilesSet">Set of original files</param>
        /// <param name="actualFilesSet">Set of files currently in the directory</param>
        /// <returns>Files that were added</returns>
        private static HashSet<VersionedFile> processAddedFiles(string basePath, HashSet<VersionedFile> originalFilesSet, HashSet<VersionedFile> actualFilesSet)
        {
            var addedFiles = new HashSet<VersionedFile>(actualFilesSet);
            addedFiles.ExceptWith(originalFilesSet);
            foreach (var file in addedFiles)
            {
                var hash = getHash(Path.Combine(basePath, file.Name));
                file.Hash = hash;
                file.Version = 1;
                file.Status = FileStatus.ADDED;
            }

            return addedFiles;
        }

        /// <summary>
        /// Gets a set of all files that were deleted
        /// </summary>
        /// <param name="originalFilesSet">Set of original files</param>
        /// <param name="actualFilesSet">Set of files currently in the directory</param>
        /// <returns>Files that were deleted</returns>
        private static HashSet<VersionedFile> processDeletedFiles(HashSet<VersionedFile> originalFilesSet, HashSet<VersionedFile> actualFilesSet)
        {
            var deletedFiles = new HashSet<VersionedFile>(originalFilesSet);
            deletedFiles.ExceptWith(actualFilesSet);
            foreach (var file in deletedFiles)
            {
                file.Status = FileStatus.DELETED;
            }

            return deletedFiles;
        }

        /// <summary>
        /// Checks all files that existed before and still exist if they were modified (using comparison of checksums)
        /// </summary>
        /// <param name="basePath">Directory to process</param>        
        /// <param name="originalFilesSet">Set of original files and their hashes</param>
        /// <param name="actualFilesSet">Set of files currently in the directory</param>
        /// <returns>Files that were neither added or deleted, with Status field containing whether they've been modified</returns>
        private static HashSet<VersionedFile> processPossibleModifications(string basePath, HashSet<VersionedFile> originalFilesSet, HashSet<VersionedFile> actualFilesSet)
        {
            var possiblyModifiedFiles = new HashSet<VersionedFile>(originalFilesSet);
            possiblyModifiedFiles.IntersectWith(actualFilesSet);
            foreach (var file in possiblyModifiedFiles)
            {

                var hash = getHash(Path.Combine(basePath, file.Name));
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

            return possiblyModifiedFiles;
        }


        /// <summary>
        /// Analyzes a new directory and prepares versioning for all files
        /// </summary>
        /// <param name="basePath">Directory to analyze</param>
        /// <param name="versioningFilePath">Path to store versioning file</param>
        /// <returns></returns>
        private static IList<VersionedFile> processNewDir(string basePath, string versioningFilePath)
        {
            
            var processedFiles = new List<VersionedFile>();
            string[] allFiles = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories);            

            foreach (var fullFilePath in allFiles)
            {
                var hash = getHash(fullFilePath);
                var versionedFile = new VersionedFile() { Name = Path.GetRelativePath(basePath, fullFilePath), Hash = hash, Status = FileStatus.ADDED, Version = 1 };
                processedFiles.Add(versionedFile);
            }


            var options = new JsonSerializerOptions { WriteIndented = true };
            using var fileStream = File.Create(versioningFilePath);            
            JsonSerializer.SerializeAsync(fileStream, processedFiles, options);            
            return processedFiles;
        }

        /// <summary>
        /// Calculates md5 hash - checksum - of given file and converts it to "usual" string representation
        /// </summary>        
        /// <param name="filename">Path to a file</param>
        /// <returns></returns>
        private static string getHash(string filename)
        {
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
