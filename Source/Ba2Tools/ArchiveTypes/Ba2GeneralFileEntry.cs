﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools.ArchiveTypes
{
    /// <summary>
    /// Represents file entry in general BA2 archive.
    /// </summary>
    /// <seealso cref="Ba2GeneralArchive"/>
    public struct Ba2GeneralFileEntry
    {
        public UInt32 Unknown0;

        /// <summary>
        /// File extension.
        /// </summary>
        public char[] Extension;

        public UInt32 Unknown1;

        public UInt32 Unknown2;

        /// <summary>
        /// Offset in archive where file data is laid.
        /// </summary>
        public UInt64 Offset;

        /// <summary>
        /// ZLib packed length. Equals 0 if not compressed.
        /// </summary>
        public UInt32 PackedLength;

        /// <summary>
        /// Real file size.
        /// </summary>
        public UInt32 UnpackedLength;

        public UInt32 Unknown3;

        /// <summary>
        /// Checks file entry being compressed.
        /// </summary>
        /// <returns>True if file is compressed, false otherwise.</returns>
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