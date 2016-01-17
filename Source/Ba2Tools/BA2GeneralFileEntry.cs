using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Represents file entry in general BA2 archive.
    /// </summary>
    /// <seealso cref="BA2GeneralArchive"/>
    public struct BA2GeneralFileEntry
    {
        /// <summary>
        /// Size of structure.
        /// </summary>
        public readonly static int Size = 4 + 4 + 4 + 4 + 8 + 4 + 4 + 4;

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
        public bool IsCompressed() => (PackedLength != 0);
    }
}
