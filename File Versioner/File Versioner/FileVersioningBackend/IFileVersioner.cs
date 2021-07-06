using FileVersioningTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileVersioningTool.FileVersioningBackend
{
    public interface IFileVersioner
    {
        public Task<IList<VersionedFile>> GetChangesListAsync(string basePath);
    }
}
