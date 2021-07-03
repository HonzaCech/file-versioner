using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileVersioningTool.FileVersioningBackend
{
    public class VersionedFile
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public int Version { get; set; }
        public FileStatus Status { get; set; }
    }
}
