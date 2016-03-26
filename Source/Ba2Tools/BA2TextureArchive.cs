using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Ba2Tools.Internal;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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

        #region Extract methods

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <param name="destination">Destination directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public override void ExtractAll(string destination, bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(fileEntries, destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token.
        /// </summary>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractAll(string destination, CancellationToken cancellationToken, bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(fileEntries, destination, cancellationToken, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token and progress reporter.
        /// </summary>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive's total files count.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public override void ExtractAll(string destination, CancellationToken cancellationToken, IProgress<int> progress, bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(fileEntries, destination, cancellationToken, progress, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(
                ConstructEntriesFromIndexes(GetIndexesFromFilenames(fileNames)),
                destination, 
                CancellationToken.None,
                null,
                overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractFiles(IEnumerable<string> fileNames, string destination, CancellationToken cancellationToken, bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(
                ConstructEntriesFromIndexes(GetIndexesFromFilenames(fileNames)),
                destination, 
                CancellationToken.None, 
                null,
                overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to <c>fileNames.Count()</c>.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="BA2ExtractionException"></exception>
        public override void ExtractFiles(
            IEnumerable<string> fileNames,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(
                ConstructEntriesFromIndexes(GetIndexesFromFilenames(fileNames)),
                destination, 
                cancellationToken,
                progress,
                overwriteFiles);
        }

        /// <summary>
        /// Extract's specified files, accessed by index to the
        /// specified directory.
        /// </summary>
        /// <param name="indexes">The indexes.</param>
        /// <param name="destination">
        /// Destination folder where extracted files will be placed.
        /// </param>
        /// <param name="overwriteFiles">Overwrite files in destination folder?</param>
        public override void ExtractFiles(IEnumerable<int> indexes, string destination, bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(
                ConstructEntriesFromIndexes(indexes),
                destination,
                CancellationToken.None,
                null,
                overwriteFiles);
        }

        /// <summary>
        /// Extracts specified files, accessed by index to the specified
        /// directory with cancellation token and progress reporter.
        /// </summary>
        /// <param name="indexes">The indexes.</param>
        /// <param name="destination">
        /// Destination folder where extracted files will be placed.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token. Set it to <c>CancellationToken.None</c>
        /// if you don't wanna cancel operation.
        /// </param>
        /// <param name="progress">
        /// Progress reporter ranged from 0 to <c>indexes.Count()</c>.
        /// Set it to <c>null</c> if you don't want to handle progress
        /// of operation.
        /// </param>
        /// <param name="overwriteFiles">Overwrite files in destination folder?</param>
        public override void ExtractFiles(
            IEnumerable<int> indexes,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false)
        {
            this.ExtractFilesInternal(
                ConstructEntriesFromIndexes(indexes),
                destination,
                cancellationToken,
                progress,
                overwriteFiles);
        }

        /// <summary>
        /// Extract file contents to stream.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <c>stream</c> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <c>fileName</c> is null or whitespace.
        /// </exception>
        public override bool ExtractToStream(string fileName, Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException(nameof(fileName));

            BA2TextureFileEntry entry = null;
            if (!TryGetEntryFromName(fileName, out entry))
                return false;

            ExtractToStreamInternal(entry, stream);
            return true;
        }

        /// <summary>
        /// Extract file, accessed by index, to the stream.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Always returns true, catch exception.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <c>index</c> is less than 0 or more than total files in archive.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <c>stream</c> is null.
        /// </exception>
        public override bool ExtractToStream(int index, Stream stream)
        {
            if (index < 0 || index > this.TotalFiles)
                throw new IndexOutOfRangeException(nameof(index));
            if (stream == null)
                throw new ArgumentException(nameof(stream));

            ExtractToStreamInternal(fileEntries[index], stream);
            return true;
        }

        /// <summary>
        /// Extract single file to specified directory by index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="destination">Absolute or relative directory path where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        /// <exception cref="IndexOutOfRangeException">
        /// <c>index</c> is less than 0 or more than total files in archive.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <c>destination</c> is null or whitespace.
        /// </exception>
        /// <exception cref="BA2ExtractionException"></exception>
        public override void Extract(int index, string destination, bool overwriteFile = false)
        {
            if (index < 0 || index > this.TotalFiles)
                throw new IndexOutOfRangeException(nameof(index));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException(nameof(destination));

            BA2TextureFileEntry entry = fileEntries[index];
            string extractPath = CreateDirectoryAndGetPath(entry, destination, overwriteFile);

            using (var stream = File.Create(extractPath, 4096, FileOptions.SequentialScan))
                ExtractToStreamInternal(entry, stream);
        }

        /// <summary>
        /// Extract single file from archive.
        /// </summary>
        /// <param name="fileName">File path, directories separated with backslash (\)</param>
        /// <param name="destination">Destination directory where file will be extracted to.</param>
        /// <param name="overwriteFile">Overwrite existing file with extracted one?</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="BA2ExtractionException">
        /// Overwrite is not permitted.
        /// </exception>
        public override void Extract(string fileName, string destination, bool overwriteFile = false)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (_fileListCache == null)
                ListFiles(true);

            BA2TextureFileEntry entry = null;
            if (!TryGetEntryFromName(fileName, out entry))
                throw new BA2ExtractionException($"Cannot find file name \"{fileName}\" in archive");

            ExtractInternal(entry, destination, overwriteFile);
        }
        #endregion

        #region Private methods

        private void ExtractFilesInternal(
            BA2TextureFileEntry[] entries,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles)
        {
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException(nameof(destination));

            int totalEntries = entries.Count();

            bool shouldUpdate = cancellationToken != null || progress != null;

            int counter = 0;
            int updateFrequency = Math.Max(1, totalEntries / 100);
            int nextUpdate = updateFrequency;

            BlockingCollection<string> readyFilenames = new BlockingCollection<string>(totalEntries);

            Action createDirs = () =>
                CreateDirectoriesForFiles(entries, readyFilenames, cancellationToken, destination, overwriteFiles);

            if (MultithreadedExtract)
                Task.Run(createDirs, cancellationToken);
            else
                createDirs();
            
            for (int i = 0; i < totalEntries; i++)
            {
                BA2TextureFileEntry entry = entries[i];
                using (var stream = File.Create(readyFilenames.Take(), 4096, FileOptions.SequentialScan))
                {
                    ExtractToStreamInternal(entry, stream);
                }

                counter++;
                if (shouldUpdate && counter >= nextUpdate)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(counter);
                    nextUpdate += updateFrequency;
                }
            }
        }

        private void ExtractInternal(BA2TextureFileEntry entry, string destinationFolder, bool overwriteFile)
        {
            string filePath = _fileListCache[entry.Index];

            string extension = new string(entry.Extension).Trim('\0');
            string finalPath = Path.Combine(destinationFolder, filePath);

            string finalDest = Path.GetDirectoryName(finalPath);
            Directory.CreateDirectory(finalDest);

            if (overwriteFile == false && File.Exists(finalPath))
                throw new BA2ExtractionException("Overwrite is not permitted.");

            using (var fileStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
            {
                ExtractToStreamInternal(entry, fileStream);
            }
        }

        private BA2TextureFileEntry[] GetFileEntries(IEnumerable<string> fileNames)
        {
            if (fileNames == null)
                throw new ArgumentNullException(nameof(fileNames));

            BA2TextureFileEntry[] entries = new BA2TextureFileEntry[fileNames.Count()];
            BA2TextureFileEntry entry;

            int i = 0;
            foreach (string name in fileNames)
            {
                if (!TryGetEntryFromName(name, out entry))
                    throw new BA2ExtractionException($"File \"{name}\" is not found in archive");

                entries[i] = entry;
                i++;
            }

            return entries;
        }

        /// <summary>
        /// Reads the chunks for entry.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="entry">The entry.</param>
        private void ReadChunksForEntry(BinaryReader reader, BA2TextureFileEntry entry)
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
        /// <param name="entry">The entry.</param>
        /// <returns>True if entry is found and populated, false otherwise.</returns>
        private bool TryGetEntryFromName(string fileName, out BA2TextureFileEntry entry)
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
        /// <param name="entry">The entry.</param>
        /// <param name="destStream">Destination stream where ready texture will be placed.</param>
        /// <remarks>
        /// No validation of arguments performed.
        /// </remarks>
        private void ExtractToStreamInternal(BA2TextureFileEntry entry, Stream destStream)
        {
            using (BinaryWriter writer = new BinaryWriter(destStream, Encoding.ASCII, leaveOpen: true))
            {
                DdsHeader header = CreateDdsHeaderForEntry(entry);

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
                    writer.Write(0U);
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
                    writer.Write(0U);
                }

                const long zlibHeaderSize = 2;
                for (uint i = 0; i < entry.NumberOfChunks; i++)
                {
                    var chunk = entry.Chunks[i];

                    ArchiveStream.Seek((long)chunk.Offset + zlibHeaderSize, SeekOrigin.Begin);

                    byte[] destBuffer = new byte[chunk.UnpackedLength];
                    using (var uncompressStream = new DeflateStream(ArchiveStream, CompressionMode.Decompress, leaveOpen: true))
                    {
                        var bytesReaden = uncompressStream.Read(destBuffer, 0, (int)chunk.UnpackedLength);
                        // Debug.Assert(bytesReaden == chunk.UnpackedLength);
                    }

                    writer.Write(destBuffer, 0, (int)chunk.UnpackedLength);
                }

                // writer.Seek(0, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Creates valid DDS Header for entry's texture.
        /// </summary>
        /// <param name="entry">Valid BA2TextureFileEntry instance.</param>
        /// <returns>
        /// Valid DDS Header.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Entry DDS format is not supported.</exception>
        private DdsHeader CreateDdsHeaderForEntry(BA2TextureFileEntry entry)
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
                    throw new NotSupportedException($"DDS format \"{format.ToString()}\" is not supported.");
            }

            return header;
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fileEntries = null;
                _fileListCache = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Preloads the data.
        /// </summary>
        /// <param name="reader">The reader.</param>
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
                    Unknown3 = reader.ReadUInt16(),
                    Index = i
                };

                ReadChunksForEntry(reader, entry);

                fileEntries[i] = entry;
            }
        }

        private BA2TextureFileEntry[] ConstructEntriesFromIndexes(IEnumerable<int> indexes)
        {
            BA2TextureFileEntry[] entries = new BA2TextureFileEntry[indexes.Count()];
            int i = 0;
            foreach (int index in indexes)
            {
                entries[i] = fileEntries[index];
                i++;
            }

            return entries;
        }
    }
}
