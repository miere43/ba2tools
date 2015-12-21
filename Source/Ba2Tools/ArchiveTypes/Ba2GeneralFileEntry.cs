using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools.ArchiveTypes
{
    //[StructLayout(LayoutKind.Auto, Pack = 1)]
    public struct Ba2GeneralFileEntry
    {
        public UInt32 Unknown0;

        public char[] Extension;

        public UInt32 Unknown1;

        public UInt32 Unknown2;

        public UInt64 Offset;

        public UInt32 PackedLength;

        public UInt32 UnpackedLength;

        public UInt32 Unknown3;

        public bool IsCompressed()
        {
            return (PackedLength != 0);
        }

        //public UInt32 GetDataLength()
        //{
        //    return IsCompressed() ? PackedLength : UnpackedLength;
        //}
    }
}
