using System;

namespace Ba2Tools.Internal
{
    /// <summary>
    /// Represents BA2TextureArchive file entry.
    /// <seealso cref="BA2TextureArchive" />
    /// </summary>
    public class BA2TextureFileEntry : IBA2FileEntry
    {
        /// <summary>
        /// Probably file hash.
        /// </summary>
        public UInt32 Unknown0 { get; internal set; }

        /// <summary>
        /// File extension. Must contain 4 chars.
        /// </summary>
        public char[] Extension { get; set; }

        /// <summary>
        /// Probably directory hash.
        /// </summary>
        public UInt32 Unknown1 { get; internal set; }

        public byte Unknown2 { get; internal set; }

        /// <summary>
        /// Number of chunks in texture.
        /// </summary>
        public byte NumberOfChunks { get; internal set; }

        /// <summary>
        /// Header size of each chunk.
        /// </summary>
        public UInt16 ChunkHeaderSize { get; internal set; }

        /// <summary>
        /// Texture height in pixels.
        /// </summary>
        public UInt16 TextureHeight { get; internal set; }

        /// <summary>
        /// Texture width in pixels.
        /// </summary>
        public UInt16 TextureWidth { get; internal set; }

        /// <summary>
        /// Number of mipmaps.
        /// </summary>
        public byte NumberOfMipmaps { get; internal set; }

        /// <summary>
        /// Texture format (DXGI_FORMAT).
        /// </summary>
        public byte Format { get; internal set; }

        /// <summary>
        /// File entry ending, always 0x0800
        /// </summary>
        public UInt16 Unknown3 { get; internal set; }

        /// <summary>
        /// Texture chunks in file.
        /// </summary>
        public TextureChunk[] Chunks { get; internal set; }

        /// <summary>
        /// Index in archive.
        /// </summary>
        public int Index { get; set; }
    }
}
