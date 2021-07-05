using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileVersioningTool.FileVersioningBackend
{
    public interface IFileVersioner
    {
        public IList<VersionedFile> GetChangesList(string basePath);
    }
}
