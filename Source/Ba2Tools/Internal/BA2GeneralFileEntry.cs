using System;

namespace Ba2Tools.Internal
{
    /// <summary>
    /// Represents file entry in general BA2 archive.
    /// </summary>
    /// <seealso cref="BA2GeneralArchive"/>
    public class BA2GeneralFileEntry : IBA2FileEntry
    {
        /// <summary>
        /// Size of structure.
        /// </summary>
        public readonly static int Size = 4 + 4 + 4 + 4 + 8 + 4 + 4 + 4;

        /// <summary>
        /// Probably file hash.
        /// </summary>
        public UInt32 Unknown0;

        /// <summary>
        /// File extension.
        /// </summary>
        public char[] Extension;

        /// <summary>
        /// Probably directory hash.
        /// </summary>
        public UInt32 Unknown1;

        /// <summary>
        /// Probably file flags.
        /// </summary>
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

        /// <summary>
        /// File entry ending, always 0xBAADF00D
        /// </summary>
        public UInt32 Unknown3;

        /// <summary>
        /// Checks file entry being compressed.
        /// </summary>
        /// <returns>True if file is compressed, false otherwise.</returns>
        public bool IsCompressed() => (PackedLength != 0);

        /// <summary>
        /// Index in archive.
        /// </summary>
        public int Index { get; set; }
    }
}
