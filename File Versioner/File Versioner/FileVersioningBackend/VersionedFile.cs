using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public override bool Equals(object obj)
        {
            if (obj is VersionedFile otherF)
            {
                return StringComparer.InvariantCultureIgnoreCase.Equals(Name, otherF.Name);
            }
            return false;
                

        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name);
        }
    }

}
