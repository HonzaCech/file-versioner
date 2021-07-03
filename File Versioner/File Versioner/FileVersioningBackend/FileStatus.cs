using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileVersioningTool.FileVersioningBackend
{
    public enum FileStatus
    {
        DELETED,
        ADDED,
        MODIFIED,
        UNCHANGED
    }
}
