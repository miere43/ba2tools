using Ba2Tools.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools.ArchiveTypes
{
    /// <summary>
    /// Represents texture BA2 archive type.
    /// </summary>
    /// <remarks>Should not be used, work in progress.</remarks>
    public sealed class Ba2TextureArchive : Ba2ArchiveBase
    {
        private void ExtractToStream(ref Ba2TextureFileEntry entry, Stream sourceStream, Stream destStream)
        {
            for (uint i = 0; i < entry.NumberOfChunks; i++)
            {
                var chunk = entry.Chunks[i];

                // byte[] sourceBuffer = new byte[chunk.PackedLength];

                sourceStream.Seek((long)chunk.Offset, SeekOrigin.Begin);
                // sourceStream.Read(sourceBuffer, 0, (int)chunk.PackedLength);

                //using (var uncompressStream = new DeflateStream(sourceStream, CompressionMode.Decompress, leaveOpen: true))
                //{
                //    var bytesReaden = uncompressStream.Read(rawData, 0, (int)dataLength);
                //    Debug.Assert(bytesReaden == dataLength);
                //}
            }
        }

        private void FillDdsHeader(ref DdsHeader header, ref Ba2TextureFileEntry entry)
        {
            DxgiFormat format = (DxgiFormat)entry.Format;

            header.dwSize = 124; // sizeof(DDS_HEADER)
            header.dwHeaderFlags = Dds.DDS_HEADER_FLAGS_TEXTURE | 
                Dds.DDS_HEADER_FLAGS_LINEARSIZE | Dds.DDS_HEADER_FLAGS_MIPMAP;
            header.dwHeight = (uint)entry.TextureHeight;
            header.dwWidth = (uint)entry.TextureWidth;
            header.dwMipMapCount = (uint)entry.NumberOfMipmaps;
            header.ddspf.dwSize = 32; // sizeof(DDS_PIXELFORMAT);
            header.dwSurfaceFlags = Dds.DDS_SURFACE_FLAGS_TEXTURE | Dds.DDS_SURFACE_FLAGS_MIPMAP;

            switch (format)
            {
                case DxgiFormat.BC1_UNORM:
                    header.ddspf.dwFlags = Dds.DDS_FOURCC;
                    header.ddspf.dwFourCC = (uint)Dds.MakeFourCC('D', 'X', 'T', '1');
                    header.dwPitchOrLinearSize = (uint)entry.TextureWidth * (uint)entry.TextureHeight / 2u;
                    break;
                case DxgiFormat.BC2_UNORM:
                    header.ddspf.dwFlags = Dds.DDS_FOURCC;
                    header.ddspf.dwFourCC = (uint)Dds.MakeFourCC('D', 'X', 'T', '3');
                    header.dwPitchOrLinearSize = (uint)entry.TextureWidth * (uint)entry.TextureHeight;
                    break;
                case DxgiFormat.BC3_UNORM:
                    header.ddspf.dwFlags = Dds.DDS_FOURCC;
                    header.ddspf.dwFourCC = (uint)Dds.MakeFourCC('D', 'X', 'T', '5');
                    header.dwPitchOrLinearSize = (uint)entry.TextureWidth * (uint)entry.TextureHeight;
                    break;
                case DxgiFormat.BC5_UNORM:
                    header.ddspf.dwFlags = Dds.DDS_FOURCC;
                    // ATI2
                    header.ddspf.dwFourCC = (uint)Dds.MakeFourCC('D', 'X', 'T', '5');
                    header.dwPitchOrLinearSize = (uint)entry.TextureWidth * (uint)entry.TextureHeight;
                    break;
                case DxgiFormat.BC7_UNORM:
                    header.ddspf.dwFlags = Dds.DDS_FOURCC;
                    header.ddspf.dwFourCC = (uint)Dds.MakeFourCC('B', 'C', '7', '\0');
                    header.dwPitchOrLinearSize = (uint)entry.TextureWidth * (uint)entry.TextureHeight;
                    break;
                case DxgiFormat.B8G8R8A8_UNORM:
                    header.ddspf.dwFlags = Dds.DDS_RGBA;
                    header.ddspf.dwRGBBitCount = 32;
                    header.ddspf.dwRBitMask = 0x00FF0000;
                    header.ddspf.dwGBitMask = 0x0000FF00;
                    header.ddspf.dwBBitMask = 0x000000FF;
                    header.ddspf.dwABitMask = 0xFF000000;
                    header.dwPitchOrLinearSize = (uint)entry.TextureWidth * (uint)entry.TextureHeight * 4u;
                    break;
                case DxgiFormat.R8_UNORM:
                    header.ddspf.dwFlags = Dds.DDS_RGB;
                    header.ddspf.dwRGBBitCount = 8;
                    header.ddspf.dwRBitMask = 0xFF;
                    header.dwPitchOrLinearSize = (uint)entry.TextureWidth * (uint)entry.TextureHeight;
                    break;
                default:
                    throw new NotImplementedException("DDS format " + format.ToString() + " is not supported.");
            }
        }
    }
}
