using System;
using System.Collections.Generic;
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
            //if (!Directory.Exists(destination))
            //    Directory.CreateDirectory(destination);
        }

        private Ba2GeneralFileEntry GetEntryFromName(ref string fileName)
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

            throw new NotSupportedException();
        }

        /// <summary>
        /// Extract single file from archive;
        /// </summary>
        /// <param name="fileName">Filename in archive</param>
        /// <param name="destination">Destination directory</param>
        /// <param name="overwriteFile">Overwrite destination file if exists? Default: false</param>
        public override void Extract(string fileName, string destination, bool overwriteFile = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException(nameof(fileName) + " is invalid");
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException(nameof(destination) + " is invalid");

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            if (_fileListCache == null)
                ListFiles(true);

            // fileEntries[] position
            int fileEntryPosition = -1;
            // Should replace this with hash search
            for (int i = 0; i < _fileListCache.Length; i++)
            {
                string name = _fileListCache[i];

                if (name.Equals(fileName, StringComparison.OrdinalIgnoreCase)) {
                    fileEntryPosition = i;
                    break;
                }
            }

            if (fileEntryPosition == -1)
                throw new Ba2ArchiveExtractionException("Cannot find file name " + fileName + " in archive");

            var fileEntry = fileEntries[fileEntryPosition];
            bool isPacked = (fileEntry.PackedLength != 0);

            string extension = new string(fileEntry.Extension).Trim('\0');
            string finalPath = Path.Combine(destination, fileName);

            string finalDest = Path.GetDirectoryName(finalPath);
            if (!Directory.Exists(finalDest))
                Directory.CreateDirectory(finalDest);

            if (File.Exists(finalPath) && overwriteFile == false)
                throw new Ba2ArchiveExtractionException("Overwrite is not permitted.");

            byte[] rawData = null;

            using (var archiveStream = File.OpenRead(FilePath))
            {
                archiveStream.Seek((long)fileEntry.Offset, SeekOrigin.Begin);

                var bytesToRead = isPacked ? (int)fileEntry.PackedLength : (int)fileEntry.UnpackedLength;
                rawData = new byte[bytesToRead];
                archiveStream.Read(rawData, 0, bytesToRead);
            }

            ExtractFileInternal(rawData, ref fileEntry, ref finalPath);
        }

        private void ExtractFileInternal(byte[] data, ref Ba2GeneralFileEntry fileEntry, ref string destFilename)
        {
            UInt64 dataOffset = fileEntry.Offset;
            UInt32 packedLength = fileEntry.PackedLength;
            UInt32 unpackedLength = fileEntry.UnpackedLength;

            bool isPacked = (packedLength != 0);

            using (var extractedFileStream = File.Create(destFilename, 4096, FileOptions.SequentialScan)) {
                if (isPacked)
                {
                    using (var uncompressStream = new DeflateStream(extractedFileStream, CompressionMode.Decompress))
                    {
                        var decompressedData = new byte[uncompressStream.Length];
                        var bytesReaden = uncompressStream.Read(decompressedData, 0, (int)uncompressStream.Length);

                        extractedFileStream.Write(decompressedData, 0, decompressedData.Length);
                        extractedFileStream.Flush();
                        extractedFileStream.Close();

                        System.Diagnostics.Debug.Assert(bytesReaden == unpackedLength);
                    }
                }
                else
                {
                    extractedFileStream.Write(data, 0, data.Length);
                    extractedFileStream.Flush();
                    extractedFileStream.Close();
                }
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
