using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ba2Tools
{
    public struct BA2Header
    {
        public byte[] Signature;

        public UInt32 Version;

        public byte[] ArchiveType;

        public UInt32 TotalFiles;

        public UInt64 NameTableOffset;
    }
}
