using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ba2Tools.ArchiveTypes
{
    public struct Ba2TextureFileEntry
    {
        public UInt32 Unknown0;

        public char[] Extension;

        public UInt32 Unknown1;

        public byte Unknown2;

        public UInt16 NumberOfChunks;

        public UInt16 ChunkHeaderSize;

        public UInt16 TextureHeight;

        public UInt16 TextureWidth;

        public byte NumberOfMipmaps;

        public byte Format;

        public UInt16 Unknown3;

        public TextureChunk[] Chunks;
    }
}
