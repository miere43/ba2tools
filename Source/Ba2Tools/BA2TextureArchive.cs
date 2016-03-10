using Ba2Tools.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Represents texture archive type.
    /// </summary>
    public sealed class BA2TextureArchive : BA2Archive
    {
        /// <summary>
        /// File entries in archive. Length of array equals to header.TotalFiles.
        /// </summary>
        private BA2TextureFileEntry[] fileEntries = null;

        /// <summary>
        /// Extracts all textures from archive to destination folder.
        /// </summary>
        /// <param name="destination">Folder where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files with extracted in destination directory?</param>
        public override void ExtractAll(string destination, bool overwriteFiles = false)
        {
            if (_fileListCache == null)
                ListFiles();

            foreach (var fileName in _fileListCache)
            {
                Extract(fileName, destination, overwriteFiles);
            }
        }

        /// <summary>
        /// Tries to extract texture from archive to stream.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <returns>
        /// Returns true when extraction went successful, false when
        /// <c>stream</c> doesn't support write or when cannot retrieve
        /// BA2TextureFileEntry from <c>fileName</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <c>fileName</c> or <c>stream</c> is null.</exception>
        public override bool ExtractToStream(string fileName, Stream stream)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanWrite)
                return false;

            BA2TextureFileEntry entry = null;
            if (!GetEntryFromName(fileName, out entry))
                return false;

            ExtractToStream(entry, stream);
            return true;
        }

        public override void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles = false)
        {
            if (fileNames == null)
                throw new ArgumentNullException("fileNames is null");
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("destination is invalid");
            if (fileNames.Count() > TotalFiles)
                throw new BA2ExtractionException("fileNames length is more than total files in archive");

            foreach (var name in fileNames)
                Extract(name, destination, false);
        }

        /// <summary>
        /// Extracts texture from archive to destination path.
        /// </summary>
        /// <param name="fileName">File name or file path in archive.</param>
        /// <param name="destination"></param>
        /// <param name="overwriteFile"></param>
        /// <seealso cref="BA2Archive.ListFiles(bool)"/>
        /// <exception cref="ArgumentException">
        /// Thrown when fileName or destination is null.
        /// </exception>
        /// <exception cref="BA2ExtractionException">
        /// Thrown when no matching file in archive was not found.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when some kind of error occur during archive loading or texture saving.
        /// </exception>
        public override void Extract(string fileName, string destination, bool overwriteFile = false)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            if (_fileListCache == null)
                ListFiles(true);

            BA2TextureFileEntry entry = null;
            if (!GetEntryFromName(fileName, out entry))
                throw new BA2ExtractionException("Cannot find file name \"" + fileName + "\" in archive");

            string extension = new string(entry.Extension).Trim('\0');
            string finalPath = Path.Combine(destination, fileName);

            string finalDest = Path.GetDirectoryName(finalPath);
            if (!Directory.Exists(finalDest))
                Directory.CreateDirectory(finalDest);

            if (File.Exists(finalPath) && overwriteFile == false)
                throw new BA2ExtractionException("Overwrite is not permitted.");

            using (var fileStream = File.OpenWrite(finalPath))
            {
                ExtractToStream(entry, fileStream);
            }
        }

        /// <summary>
        /// Preload file entries. Should be called only once.
        /// </summary>
        /// <param name="reader">BinaryReader instance</param>
        internal override void PreloadData(BinaryReader reader)
        {
            reader.BaseStream.Seek(BA2Loader.HeaderSize, SeekOrigin.Begin);
            fileEntries = new BA2TextureFileEntry[TotalFiles];

            for (int i = 0; i < TotalFiles; i++)
            {
                BA2TextureFileEntry entry = new BA2TextureFileEntry()
                {
                    Unknown0 = reader.ReadUInt32(),
                    Extension = Encoding.ASCII.GetChars(reader.ReadBytes(4)),
                    Unknown1 = reader.ReadUInt32(),
                    Unknown2 = reader.ReadByte(),
                    NumberOfChunks = reader.ReadByte(),
                    ChunkHeaderSize = reader.ReadUInt16(),
                    TextureHeight = reader.ReadUInt16(),
                    TextureWidth = reader.ReadUInt16(),
                    NumberOfMipmaps = reader.ReadByte(),
                    Format = reader.ReadByte(),
                    Unknown3 = reader.ReadUInt16()
                };

                ReadChunksForEntry(reader, ref entry);

                fileEntries[i] = entry;
            }
        }

        #region Private methods
        /// <summary>
        /// Reads all chunks for entry. Should be called after reading BA2TextureFileEntry from archive.
        /// </summary>
        /// <param name="reader">BinaryReader instance.</param>
        /// <param name="entry"></param>
        private void ReadChunksForEntry(BinaryReader reader, ref BA2TextureFileEntry entry)
        {
            var chunks = new TextureChunk[entry.NumberOfChunks];

            for (int i = 0; i < entry.NumberOfChunks; i++)
            {
                TextureChunk chunk = new TextureChunk()
                {
                    Offset = reader.ReadUInt64(),
                    PackedLength = reader.ReadUInt32(),
                    UnpackedLength = reader.ReadUInt32(),
                    StartMipmap = reader.ReadUInt16(),
                    EndMipmap = reader.ReadUInt16(),
                    Unknown = reader.ReadUInt32()
                };

                chunks[i] = chunk;
            }

            entry.Chunks = chunks;
        }

        /// <summary>
        /// Retrieves BA2TextureFileEntry from archive file name.
        /// </summary>
        /// <param name="fileName">File name in archive.</param>
        /// <returns>
        /// Nullable BA2TextureFileEntry. 
        /// Contains null if matching entry for file name was not found.
        /// </returns>
        private bool GetEntryFromName(string fileName, out BA2TextureFileEntry entry)
        {
            if (_fileListCache == null)
                ListFiles();

            int index = _fileListCache.FindIndex(x => x.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (index == -1)
            {
                entry = null;
                return false;
            }

            entry = fileEntries[index];
            return true;
        }

        /// <summary>
        /// Extracts and decompresses texture data, then combines it in valid DDS texture, then writes it to destination stream.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="archiveStream">Archive stream.</param>
        /// <param name="destStream">Destination stream where ready texture will be placed.</param>
        /// <remarks>No validation of arguments performed.</remarks>
        private void ExtractToStream(BA2TextureFileEntry entry, Stream destStream)
        {
            using (BinaryWriter writer = new BinaryWriter(destStream, Encoding.ASCII, leaveOpen: true))
            {
                DdsHeader header = CreateDdsHeaderForEntry(ref entry);

                writer.Write(Dds.DDS_MAGIC);
                writer.Write(header.dwSize);
                writer.Write(header.dwHeaderFlags);
                writer.Write(header.dwHeight);
                writer.Write(header.dwWidth);
                writer.Write(header.dwPitchOrLinearSize);
                writer.Write(header.dwDepth);
                writer.Write(header.dwMipMapCount);
                for (int i = 0; i < 11; i++)
                {
                    writer.Write((uint)0);
                }
                writer.Write(header.ddspf.dwSize);
                writer.Write(header.ddspf.dwFlags);
                writer.Write(header.ddspf.dwFourCC);
                writer.Write(header.ddspf.dwRGBBitCount);
                writer.Write(header.ddspf.dwRBitMask);
                writer.Write(header.ddspf.dwGBitMask);
                writer.Write(header.ddspf.dwBBitMask);
                writer.Write(header.ddspf.dwABitMask);
                writer.Write(header.dwSurfaceFlags);
                writer.Write(header.dwCubemapFlags);
                for (int i = 0; i < 3; i++)
                {
                    writer.Write((uint)0);
                }

                for (uint i = 0; i < entry.NumberOfChunks; i++)
                {
                    var chunk = entry.Chunks[i];

                    ArchiveStream.Seek((long)chunk.Offset + 2, SeekOrigin.Begin);

                    byte[] destBuffer = new byte[chunk.UnpackedLength];
                    using (var uncompressStream = new DeflateStream(ArchiveStream, CompressionMode.Decompress, leaveOpen: true))
                    {
                        var bytesReaden = uncompressStream.Read(destBuffer, 0, (int)chunk.UnpackedLength);
                        // Debug.Assert(bytesReaden == chunk.UnpackedLength);
                    }

                    writer.Write(destBuffer, 0, (int)chunk.UnpackedLength);
                }

                writer.Seek(0, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Creates valid DDS Header for entry's texture.
        /// </summary>
        /// <param name="entry">Valid BA2TextureFileEntry instance.</param>
        /// <returns>Valid DDS Header.</returns>
        /// <exception cref="NotSupportedException">DDS header for entries DDS format is not supported.</exception>
        private DdsHeader CreateDdsHeaderForEntry(ref BA2TextureFileEntry entry)
        {
            var header = new DdsHeader();
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
                    throw new NotSupportedException("DDS format " + format.ToString() + " is not supported.");
            }

            return header;
        }
        #endregion
    }
}
