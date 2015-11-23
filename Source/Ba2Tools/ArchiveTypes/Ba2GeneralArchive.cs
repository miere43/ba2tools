using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools.ArchiveTypes
{
    public sealed class Ba2GeneralArchive : Ba2ArchiveBase
    {
        public static readonly uint FileEntrySize = 36u;

        private Ba2GeneralFileEntry[] fileEntries = null;

        public override void ExtractAll(string destination, bool overwriteFiles = false)
        {
            ExtractFiles(ListFiles(), destination, overwriteFiles);
        }

        private Ba2GeneralFileEntry? GetEntryFromName(ref string fileName)
        {
            if (_fileListCache == null)
                ListFiles();

            for (int i = 0; i < _fileListCache.Length; i++)
            {
                string name = _fileListCache[i];

                if (name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return fileEntries[i];
                }
            }

            return null;
        }

        public override void ExtractFiles(string[] fileNames, string destination, bool overwriteFiles = false)
        {
            if (fileNames == null)
                throw new ArgumentNullException("fileNames is null");
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("destination is invalid");
            if (fileNames.Length > TotalFiles)
                throw new Ba2ArchiveExtractionException("fileNames length is more than total files in archive");

            if (fileNames.Length == 1) { 
                Extract(fileNames[0], destination, overwriteFiles);
                return;
            }

            using (var archiveStream = File.OpenRead(FilePath))
            {
                for (int i = 0; i < fileNames.Length; i++)
                {
                    var name = fileNames[i];
                    var entry = GetEntryFromName(ref name);
                    if (!entry.HasValue)
                        throw new Ba2ArchiveExtractionException("File \"" + name + "\" is not found in archive");

                    string finalFilename = Path.Combine(destination, name);
                    if (overwriteFiles == false && File.Exists(finalFilename))
                        throw new Ba2ArchiveExtractionException("File \"" + name + "\" exists.");

                    string finalDestDir = Path.GetDirectoryName(finalFilename);
                    if (!Directory.Exists(finalDestDir))
                        Directory.CreateDirectory(finalDestDir);

                    var eentry = entry.Value;
                    ExtractFileInternal(ref eentry, ref finalFilename, archiveStream);
                }
            }
        }

        /// <summary>
        /// Extract single file from archive
        /// </summary>
        /// <seealso cref="Ba2ArchiveExtractionException"/>
        /// <param name="fileName">File name in archive</param>
        /// <param name="destination">Destination directory</param>
        /// <param name="overwriteFile">Overwrite destination file if exists? Default: false</param>
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

            Ba2GeneralFileEntry? fileEntryNullable = GetEntryFromName(ref fileName);
            if (!fileEntryNullable.HasValue)
                throw new Ba2ArchiveExtractionException("Cannot find file name \"" + fileName + "\" in archive");

            var fileEntry = fileEntryNullable.Value;

            string extension = new string(fileEntry.Extension).Trim('\0');
            string finalPath = Path.Combine(destination, fileName);

            string finalDest = Path.GetDirectoryName(finalPath);
            if (!Directory.Exists(finalDest))
                Directory.CreateDirectory(finalDest);

            if (File.Exists(finalPath) && overwriteFile == false)
                throw new Ba2ArchiveExtractionException("Overwrite is not permitted.");

            using (var archiveStream = File.OpenRead(FilePath)) { 
                ExtractFileInternal(ref fileEntry, ref finalPath, archiveStream);
            }
        }

        private void ExtractFileInternal(ref Ba2GeneralFileEntry fileEntry, ref string destFilename, Stream archiveStream)
        {
            // DeflateStream throws exception when
            // reads zlib compressed file header
            const int zlibHeaderLength = 2;

            UInt64 dataOffset = fileEntry.Offset + zlibHeaderLength;
            UInt32 dataLength = fileEntry.IsCompressed() ? fileEntry.PackedLength - zlibHeaderLength : fileEntry.UnpackedLength;

            archiveStream.Seek((long)dataOffset, SeekOrigin.Begin);

            int bytesToRead = (int)dataLength;
            byte[] rawData = new byte[fileEntry.UnpackedLength];

            using (var extractedFileStream = File.Create(destFilename, 4096, FileOptions.SequentialScan))
            {
                if (fileEntry.IsCompressed())
                {
                    using (var uncompressStream = new DeflateStream(archiveStream, CompressionMode.Decompress, leaveOpen: true))
                    {
                        var bytesReaden = uncompressStream.Read(rawData, 0, (int)dataLength);
                        Debug.Assert(bytesReaden == dataLength);
                    }
                }
                else
                {
                    archiveStream.Read(rawData, 0, (int)fileEntry.UnpackedLength);
                }

                extractedFileStream.Write(rawData, 0, rawData.Length);
                extractedFileStream.Flush();
                extractedFileStream.Close();
            }
        }

        internal override void PreloadData(BinaryReader reader = null)
        {
            if (reader == null)
            reader.BaseStream.Seek(Ba2ArchiveLoader.HeaderSize, SeekOrigin.Begin);
            fileEntries = new Ba2GeneralFileEntry[TotalFiles];

            for (int i = 0; i < TotalFiles; i++)
            {
                Ba2GeneralFileEntry entry = new Ba2GeneralFileEntry()
                {
                    Unknown0       = reader.ReadUInt32(),
                    Extension      = Encoding.ASCII.GetChars(reader.ReadBytes(4)),
                    Unknown1       = reader.ReadUInt32(),
                    Unknown2       = reader.ReadUInt32(),
                    Offset         = reader.ReadUInt64(),
                    PackedLength   = reader.ReadUInt32(),
                    UnpackedLength = reader.ReadUInt32(),
                    Unknown3       = reader.ReadUInt32(),
                };

                // 3131961357 = 0xBAADF00D as uint little-endian (0x0DF0ADBA)
                System.Diagnostics.Debug.Assert(entry.Unknown3 == 3131961357);

                fileEntries[i] = entry;
            }
        }
    }
}
