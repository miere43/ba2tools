﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ba2Tools.ArchiveTypes
{
    public struct TextureChunk
    {
        public UInt64 Offset;

        public UInt32 PackedLength;

        public UInt32 UnpackedLength;

        public UInt16 StartMipmap;

        public UInt16 EndMipmap;

        // UInt32 Unknown // 0xBAADF00D
    }
}
