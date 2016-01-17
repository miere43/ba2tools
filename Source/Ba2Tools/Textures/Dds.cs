using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ba2Tools.Internal
{
    public struct DdsPixelFormat
    {
        public UInt32 dwSize;
        public UInt32 dwFlags;
        public UInt32 dwFourCC;
        public UInt32 dwRGBBitCount;
        public UInt32 dwRBitMask;
        public UInt32 dwGBitMask;
        public UInt32 dwBBitMask;
        public UInt32 dwABitMask;
    }

    public enum DxgiFormat : int
    {
        R8_UNORM = 61,
        BC1_UNORM = 71,
        BC2_UNORM = 74,
        BC3_UNORM = 77,
        BC5_UNORM = 83,
        BC7_UNORM = 98,
        B8G8R8A8_UNORM = 87,
    }

    public struct DdsHeader
    {
        public UInt32 dwSize;
        public UInt32 dwHeaderFlags;
        public UInt32 dwHeight;
        public UInt32 dwWidth;
        public UInt32 dwPitchOrLinearSize;
        public UInt32 dwDepth;
        public UInt32 dwMipMapCount;
        //public UInt32[] dwReserved;
        public DdsPixelFormat ddspf;
        public UInt32 dwSurfaceFlags;
        public UInt32 dwCubemapFlags;
        //public UInt32[] dwReserved2;
    }

    public static class Dds
    {
        public static UInt32 DDS_MAGIC = 0x20534444; // "DDS " //MakeFourCC((byte)'D', (byte)'D', (byte)'S', (byte)'\0');

        public static UInt32 DDS_RGB = 0x00000040;

        public static UInt32 DDS_RGBA = 0x00000041;

        public static UInt32 DDS_FOURCC = 0x00000004;

        public static UInt32 DDS_HEADER_FLAGS_TEXTURE = 0x00001007;

        public static UInt32 DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000;

        public static UInt32 DDS_HEADER_FLAGS_MIPMAP = 0x00020000;

        public static UInt32 DDS_SURFACE_FLAGS_TEXTURE = 0x00001000;

        public static UInt32 DDS_SURFACE_FLAGS_MIPMAP = 0x00400008;

        // https://msdn.microsoft.com/en-us/library/windows/desktop/bb153349(v=vs.85).aspx
        public static int MakeFourCC(int ch0, int ch1, int ch2, int ch3)
        {
            return ((int)(byte)(ch0) | ((int)(byte)(ch1) << 8) | ((int)(byte)(ch2) << 16) | ((int)(byte)(ch3) << 24));
        }
    }
}
