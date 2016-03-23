using System;

namespace Ba2Tools
{
    public struct BA2Header
    {
        /// <summary>
        /// Archive signature.
        /// </summary>
        public byte[] Signature;

        /// <summary>
        /// Archive version.
        /// </summary>
        public UInt32 Version;

        /// <summary>
        /// Archive type.
        /// </summary>
        public byte[] ArchiveType;

        /// <summary>
        /// Total files in archive.
        /// </summary>
        public UInt32 TotalFiles;

        /// <summary>
        /// Offset to name table.
        /// </summary>
        public UInt64 NameTableOffset;
    }
}
