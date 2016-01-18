using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ba2Tools
{
    /// <summary>
    /// Represents DDS texture chunk.
    /// </summary>
    public struct TextureChunk
    {
        /// <summary>
        /// Offset to chunk data from beginning of the archive.
        /// </summary>
        public UInt64 Offset;

        /// <summary>
        /// zlib chunk packed size.
        /// </summary>
        public UInt32 PackedLength;

        /// <summary>
        /// Unpacked size of chunk.
        /// </summary>
        public UInt32 UnpackedLength;

        public UInt16 StartMipmap;

        public UInt16 EndMipmap;

        public UInt32 Unknown; // 0xBAADF00D
    }
}
