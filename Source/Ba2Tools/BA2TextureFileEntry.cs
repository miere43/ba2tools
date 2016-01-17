using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ba2Tools
{
    /// <summary>
    /// Represents texture BA2 archive file entry.
    /// <seealso cref="BA2TextureArchive"/>
    /// </summary>
    public struct BA2TextureFileEntry
    {
        public UInt32 Unknown0;

        /// <summary>
        /// File extension.
        /// </summary>
        public char[] Extension;

        public UInt32 Unknown1;

        public byte Unknown2;

        public byte NumberOfChunks;

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
        /// Texture format.
        /// </summary>
        public byte Format;

        public UInt16 Unknown3;

        /// <summary>
        /// Texture chunks in file.
        /// </summary>
        public TextureChunk[] Chunks;
    }
}
