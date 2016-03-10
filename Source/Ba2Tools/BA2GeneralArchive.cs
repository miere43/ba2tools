using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Represents general BA2 archive type.
    /// </summary>
    public sealed class BA2GeneralArchive : BA2Archive
    {
        private BA2GeneralFileEntry[] fileEntries = null;

        #region Extract methods
        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <seealso cref="ExtractFiles(IEnumerable{string}, string, bool)"/>
        /// <seealso cref="Extract(string, string, bool)"/>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public override void ExtractAll(string destination, bool overwriteFiles = false)
        {
            this.ExtractFiles(ListFiles(), destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractAll(string destination, CancellationToken cancellationToken, bool overwriteFiles = false)
        {
            this.ExtractFiles(ListFiles(), destination, cancellationToken, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token and progress reporter.
        /// </summary>
        /// <seealso cref="ExtractFiles(IEnumerable{string}, string, bool)"/>
        /// <seealso cref="Extract(string, string, bool)"/>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive's total files count.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public override void ExtractAll(string destination, CancellationToken cancellationToken, IProgress<int> progress, bool overwriteFiles = false)
        {
            this.ExtractFiles(ListFiles(), destination, cancellationToken, progress, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles = false)
        {
            this.ExtractFiles(fileNames, destination, CancellationToken.None, null, overwriteFiles = false);
        }

        /// <summary>
        /// Extract all files from archive to destination directory
        /// with cancellation token.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractFiles(IEnumerable<string> fileNames, string destination, CancellationToken cancellationToken, bool overwriteFiles = false)
        {
            this.ExtractFiles(fileNames, destination, cancellationToken, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive total files count.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractFiles(
            IEnumerable<string> fileNames,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false)
        {
            if (fileNames == null)
                throw new ArgumentNullException("fileNames is null");
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("destination is invalid");
            if (fileNames.Count() > TotalFiles)
                throw new BA2ExtractionException("fileNames length is more than total files in archive");

            int counter = 0;
            // make ~100 updates.
            int updateFrequency = Math.Max(1, fileNames.Count() / 100);
            Debug.WriteLine("Set update freq to {0} (count: {1})", updateFrequency, fileNames.Count());
            int nextUpdate = updateFrequency;

            BA2GeneralFileEntry entry = null;
            foreach (var name in fileNames)
            {
                if (!GetEntryFromName(name, out entry))
                    throw new BA2ExtractionException("File \"" + name + "\" is not found in archive");

                string finalFilename = Path.Combine(destination, name);
                if (overwriteFiles == false && File.Exists(finalFilename))
                    throw new BA2ExtractionException("File \"" + name + "\" exists.");

                string finalDestDir = Path.GetDirectoryName(finalFilename);
                if (!Directory.Exists(finalDestDir))
                    Directory.CreateDirectory(finalDestDir);

                ExtractFileInternal(ref entry, ref finalFilename);

                counter++;
                if (counter >= nextUpdate)
                {
                    Debug.WriteLine("Update called, counter: {0}, upd: {1}", counter, nextUpdate);
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(counter);
                    nextUpdate += updateFrequency;
                }
            }
        }
        #endregion

        /// <summary>
        /// Converts file name in archive to Ba2GeneralFileEntry.
        /// </summary>
        /// <param name="fileName">Filename in archive.</param>
        /// <returns>Ba2GeneralFileEntry or null if not found.</returns>
        private bool GetEntryFromName(string fileName, out BA2GeneralFileEntry entry)
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
        /// Extract single file from archive.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileName">File name in archive</param>
        /// <param name="destination">Directory where files will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in extraction directory?</param>
        public override void Extract(string fileName, string destination, bool overwriteFile = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("fileName is invalid");
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("destination is invalid");

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            if (_fileListCache == null)
                ListFiles(true);

            BA2GeneralFileEntry entry = null;

            if (!GetEntryFromName(fileName, out entry))
                throw new BA2ExtractionException("Cannot find file name \"" + fileName + "\" in archive");

            string extension = new string(entry.Extension).Trim('\0');
            string finalPath = Path.Combine(destination, fileName);

            string finalDest = Path.GetDirectoryName(finalPath);
            if (!Directory.Exists(finalDest))
                Directory.CreateDirectory(finalDest);

            if (File.Exists(finalPath) && overwriteFile == false)
                throw new BA2ExtractionException("Overwrite is not permitted.");

            ExtractFileInternal(ref entry, ref finalPath);
        }

        public override bool ExtractToStream(string fileName, Stream stream)
        {
            if (fileName == null)
                throw new ArgumentException(nameof(fileName));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (_fileListCache == null)
                ListFiles();

            BA2GeneralFileEntry entry = null;
            if (!GetEntryFromName(fileName, out entry))
                return false;

            ExtractFileInternal2(ref entry, stream);
            return true;
        }

        private void ExtractFileInternal2(ref BA2GeneralFileEntry entry, Stream destStream)
        {
            // DeflateStream throws exception when
            // reads zlib compressed file header
            const int zlibHeaderLength = 2;

            // offset to file data
            UInt64 dataOffset = entry.IsCompressed() ? entry.Offset + zlibHeaderLength : entry.Offset;
            UInt32 dataLength = entry.IsCompressed() ? entry.PackedLength - zlibHeaderLength : entry.UnpackedLength;

            ArchiveStream.Seek((long)dataOffset, SeekOrigin.Begin);

            int bytesToRead = (int)dataLength;
            byte[] rawData = new byte[entry.UnpackedLength];

            if (entry.IsCompressed())
            {
                using (var uncompressStream = new DeflateStream(ArchiveStream, CompressionMode.Decompress, leaveOpen: true))
                {
                    var bytesReaden = uncompressStream.Read(rawData, 0, (int)dataLength);
                    Debug.Assert(bytesReaden == dataLength);
                }
            }
            else
            {
                ArchiveStream.Read(rawData, 0, (int)entry.UnpackedLength);
            }

            destStream.Write(rawData, 0, rawData.Length);
            destStream.Flush();

            destStream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Base extraction function for all extraction methods.
        /// </summary>
        private void ExtractFileInternal(ref BA2GeneralFileEntry fileEntry, ref string destFilename)
        {
            using (var stream = new MemoryStream())
            {
                ExtractFileInternal2(ref fileEntry, stream);

                using (var extractedFileStream = File.Create(destFilename, 4096, FileOptions.SequentialScan))
                {
                    extractedFileStream.Write(stream.GetBuffer(), 0, (int)stream.Length);
                    extractedFileStream.Flush();
                    extractedFileStream.Close();
                }
            }
        }

        /// <summary>
        /// Preload file entries. Should be called only once.
        /// </summary>
        /// <param name="reader"></param>
        internal override void PreloadData(BinaryReader reader)
        {
            reader.BaseStream.Seek(BA2Loader.HeaderSize, SeekOrigin.Begin);
            fileEntries = new BA2GeneralFileEntry[TotalFiles];

            for (int i = 0; i < TotalFiles; i++)
            {
                BA2GeneralFileEntry entry = new BA2GeneralFileEntry()
                {
                    Unknown0 = reader.ReadUInt32(),
                    Extension = Encoding.ASCII.GetChars(reader.ReadBytes(4)),
                    Unknown1 = reader.ReadUInt32(),
                    Unknown2 = reader.ReadUInt32(),
                    Offset = reader.ReadUInt64(),
                    PackedLength = reader.ReadUInt32(),
                    UnpackedLength = reader.ReadUInt32(),
                    Unknown3 = reader.ReadUInt32(),
                };

                // 3131961357 = 0xBAADF00D as uint little-endian (0x0DF0ADBA)
                System.Diagnostics.Debug.Assert(entry.Unknown3 == 3131961357);

                fileEntries[i] = entry;
            }
        }
    }
}
