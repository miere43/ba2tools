using System;

namespace Ba2Tools
{
    /// <summary>
    /// Represents BA2TextureArchive file entry.
    /// <seealso cref="BA2TextureArchive"/>
    /// </summary>
    public struct BA2TextureFileEntry
    {
        public UInt32 Unknown0;

        /// <summary>
        /// File extension. Must contain 4 chars.
        /// </summary>
        public char[] Extension;

        public UInt32 Unknown1;

        public byte Unknown2;

        /// <summary>
        /// Number of chunks in texture.
        /// </summary>
        public byte NumberOfChunks;

        /// <summary>
        /// Header size of each chunk.
        /// </summary>
        public UInt16 ChunkHeaderSize;

        /// <summary>
        /// Texture height in pixels.
        /// </summary>
        public UInt16 TextureHeight;

        /// <summary>
        /// Texture width in pixels.
        /// </summary>
        public UInt16 TextureWidth;

        /// <summary>
        /// Number of mipmaps.
        /// </summary>
        public byte NumberOfMipmaps;

        /// <summary>
        /// Texture format (DXGI_FORMAT)
        /// </summary>
        public byte Format;

        public UInt16 Unknown3;

        /// <summary>
        /// Texture chunks in file.
        /// </summary>
        public TextureChunk[] Chunks;
    }
}
