using System;

namespace Ba2Tools.Internal
{
    /// <summary>
    /// Represents DDS_PIXELFORMAT struct.
    /// </summary>
    /// <remarks>
    /// https://msdn.microsoft.com/ru-ru/library/windows/desktop/bb943984(v=vs.85).aspx
    /// </remarks>
    internal struct DdsPixelFormat
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

    /// <summary>
    /// Represents DXGI_FORMAT enum.
    /// </summary>
    /// <remarks>
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/bb173059(v=vs.85).aspx
    /// </remarks>
    internal enum DxgiFormat : int
    {
        R8_UNORM = 61,
        BC1_UNORM = 71,
        BC2_UNORM = 74,
        BC3_UNORM = 77,
        BC5_UNORM = 83,
        BC7_UNORM = 98,
        B8G8R8A8_UNORM = 87,
    }

    /// <summary>
    /// Represents header of DDS texture.
    /// </summary>
    /// <remarks>
    /// https://msdn.microsoft.com/ru-ru/library/windows/desktop/bb943982(v=vs.85).aspx
    /// </remarks>
    internal struct DdsHeader
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

    /// <summary>
    /// Collecion of methods and constants for DDS texture format.
    /// </summary>
    internal static class Dds
    {
        /// <summary>
        /// Magic for DDS files.
        /// Same as MakeFourCC('D', 'D', 'S', '\0');
        /// </summary>
        public static readonly UInt32 DDS_MAGIC = 0x20534444;

        public static readonly UInt32 DDS_RGB = 0x00000040;

        public static readonly UInt32 DDS_RGBA = 0x00000041;

        public static readonly UInt32 DDS_FOURCC = 0x00000004;

        public static readonly UInt32 DDS_HEADER_FLAGS_TEXTURE = 0x00001007;

        public static readonly UInt32 DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000;

        public static readonly UInt32 DDS_HEADER_FLAGS_MIPMAP = 0x00020000;

        public static readonly UInt32 DDS_SURFACE_FLAGS_TEXTURE = 0x00001000;

        public static readonly UInt32 DDS_SURFACE_FLAGS_MIPMAP = 0x00400008;

        /// <summary>
        /// Creates FourCC code for DDS header.
        /// </summary>
        /// <param name="ch0">ASCII char 1.</param>
        /// <param name="ch1">ASCII char 2.</param>
        /// <param name="ch2">ASCII char 3.</param>
        /// <param name="ch3">ASCII char 4.</param>
        /// <returns>FourCC code.</returns>
        public static int MakeFourCC(int ch0, int ch1, int ch2, int ch3)
        {
            return ((int)(byte)(ch0) | ((int)(byte)(ch1) << 8) | ((int)(byte)(ch2) << 16) | ((int)(byte)(ch3) << 24));
        }
    }
}
