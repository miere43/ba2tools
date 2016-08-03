﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Ba2Tools.Internal;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Collections.Concurrent;

namespace Ba2Tools
{
    /// <summary>
    /// Represents general BA2 archive type.
    /// </summary>
    public sealed class BA2GeneralArchive : BA2Archive
    {
        private BA2GeneralFileEntry[] fileEntries = null;

        private SemaphoreSlim accessSemaphore = new SemaphoreSlim(1, 1);

        #region BA2Archive Overrides

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <param name="destination">Destination directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public override void ExtractAll(string destination, bool overwriteFiles)
        {
            CheckDisposed();
            this.ExtractFilesInternal(fileEntries, destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with cancellation token and progress reporter.
        /// </summary>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive's total files count.</param>
        public override void ExtractAll(string destination, bool overwriteFiles, CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            CheckDisposed();
            this.ExtractFilesInternal(fileEntries, destination, cancellationToken, progress, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public override void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles)
        {
            CheckDisposed();
            this.ExtractFilesInternal(ConstructEntriesFromIndexes(GetIndexesFromFilenames(fileNames)), destination,
                CancellationToken.None, null, overwriteFiles);
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
        public override void ExtractFiles(IEnumerable<int> indexes, string destination, bool overwriteFiles)
        {
            CheckDisposed();
            this.ExtractFilesInternal(ConstructEntriesFromIndexes(indexes), destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extracts specified files, accessed by index to the specified directory with cancellation token and progress reporter.
        /// </summary>
        /// <param name="indexes">The indexes.</param>
        /// <param name="destination">
        /// Destination folder where extracted files will be placed.
        /// </param>
        /// <param name="overwriteFiles">Overwrite files in destination folder?</param>
        /// <param name="cancellationToken">
        /// The cancellation token. Set it to <c>CancellationToken.None</c> if you don't care.
        /// </param>
        /// <param name="progress">
        /// Progress reporter ranged from 0 to <c>indexes.Count()</c>. Set it to <c>null</c> if you don't want to handle progress
        /// of operation.
        /// </param>
        public override void ExtractFiles(IEnumerable<int> indexes, string destination, bool overwriteFiles,
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            CheckDisposed();
            this.ExtractFilesInternal(ConstructEntriesFromIndexes(indexes), destination, cancellationToken, progress, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to <c>fileNames.Count()</c>.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="BA2ExtractionException"></exception>
        public override void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles,
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            CheckDisposed();
            this.ExtractFilesInternal(ConstructEntriesFromIndexes(GetIndexesFromFilenames(fileNames)), destination, cancellationToken,
                progress, overwriteFiles);
        }

        /// <summary>
        /// Extract single file from archive.
        /// </summary>
        /// <param name="fileName">File path, directories separated with backslash (\)</param>
        /// <param name="destination">Destination directory where file will be extracted to.</param>
        /// <param name="overwriteFile">Overwrite existing file with extracted one?</param>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="BA2ExtractionException">
        /// Overwrite is not permitted.
        /// </exception>
        public override void Extract(string fileName, string destination, bool overwriteFile)
        {
            CheckDisposed();
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            int index = GetIndexFromFileName(fileName);
            if (index == -1)
                throw new BA2ExtractionException($"Cannot find file \"{ fileName }\" in archive.");

            this.Extract(index, destination, overwriteFile);
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
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="BA2ExtractionException"></exception>
        public override void Extract(int index, string destination, bool overwriteFile)
        {
            CheckDisposed();
            if (index < 0 || index > this.TotalFiles)
                throw new IndexOutOfRangeException(nameof(index));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            BA2GeneralFileEntry entry = fileEntries[index];
            string extractPath = CreateDirectoryAndGetPath(entry, destination, overwriteFile);

            using (var stream = File.Create(extractPath, 4096, FileOptions.SequentialScan))
            {
                try
                {
                    accessSemaphore.Wait();
                    ExtractToStreamInternal(entry, stream);
                }
                finally
                {
                    accessSemaphore.Release();
                }
            }
        }

        /// <summary>
        /// Extract file contents to stream.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// False when <c>fileName</c> is not found in archive, true otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <c>stream</c> or <c>fileName</c> is null.
        /// </exception>
        public override bool ExtractToStream(string fileName, Stream stream)
        {
            CheckDisposed();
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            int index = GetIndexFromFileName(fileName);
            if (index == -1)
                return false;

            try
            {
                accessSemaphore.Wait();
                ExtractToStreamInternal(fileEntries[index], stream);
            }
            finally
            {
                accessSemaphore.Release();
            }
            return true;
        }

        /// <summary>
        /// Extract file, accessed by index, to the stream.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <c>index</c> is less than 0 or more than total files in archive.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <c>stream</c> is null.
        /// </exception>
        public override bool ExtractToStream(int index, Stream stream)
        {
            CheckDisposed();
            if (index < 0 || index > this.TotalFiles)
                throw new IndexOutOfRangeException(nameof(index));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                accessSemaphore.Wait();
                ExtractToStreamInternal(fileEntries[index], stream);
            }
            finally
            {
                accessSemaphore.Release();
            }
            return true;
        }

        /// <summary>
        /// Preloads the data.
        /// </summary>
        /// <remarks>
        /// Do not call base.PreloadData().
        /// </remarks>
        /// <param name="reader">The reader.</param>
        internal override void PreloadData(BinaryReader reader)
        {
            CheckDisposed();

            try
            {
                accessSemaphore.Wait();

                this.BuildFileList();

                m_archiveStream.Seek(BA2Loader.HeaderSize, SeekOrigin.Begin);
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
                        Index = i
                    };

                    // 3131961357 = 0xBAADF00D as uint little-endian (0x0DF0ADBA)
                    // System.Diagnostics.Debug.Assert(entry.Unknown3 == 3131961357);

                    fileEntries[i] = entry;
                }
            }
            finally
            {
                accessSemaphore.Release();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Don't forget to wrap semaphore.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="destStream"></param>
        private void ExtractToStreamInternal(BA2GeneralFileEntry entry, Stream destStream)
        {
            // DeflateStream throws exception when
            // reads zlib compressed file header
            const int zlibHeaderLength = 2;

            // offset to file data
            UInt64 dataOffset = entry.IsCompressed() ? entry.Offset + zlibHeaderLength : entry.Offset;
            UInt32 dataLength = entry.IsCompressed() ? entry.PackedLength - zlibHeaderLength : entry.UnpackedLength;

            m_archiveStream.Seek((long)dataOffset, SeekOrigin.Begin);

            int bytesToRead = (int)dataLength;
            byte[] rawData = new byte[entry.UnpackedLength];

            if (entry.IsCompressed())
            {
                using (var uncompressStream = new DeflateStream(m_archiveStream, CompressionMode.Decompress, leaveOpen: true))
                {
                    var bytesReaden = uncompressStream.Read(rawData, 0, (int)dataLength);
                }
            }
            else
            {
                // TODO
                // entry.UnpackedLength => dataLength;
                m_archiveStream.Read(rawData, 0, (int)entry.UnpackedLength);
            }

            destStream.Write(rawData, 0, rawData.Length);
            destStream.Flush();

            destStream.Seek(0, SeekOrigin.Begin);
        }

        private void ExtractFilesInternal(BA2GeneralFileEntry[] entries, string destination, CancellationToken cancellationToken,
            IProgress<int> progress, bool overwriteFiles)
        {
            try
            {
                accessSemaphore.Wait(cancellationToken);

                if (string.IsNullOrWhiteSpace(destination))
                    throw new ArgumentException(nameof(destination));

                int totalEntries = entries.Count();

                bool shouldUpdate = cancellationToken != null || progress != null;

                int counter = 0;
                int updateFrequency = Math.Max(1, totalEntries / 100);
                int nextUpdate = updateFrequency;

                BlockingCollection<string> readyFilenames = new BlockingCollection<string>(totalEntries);

                var task = Task.Run(() =>
                {
                    CreateDirectoriesForFiles(entries, readyFilenames, cancellationToken, destination, overwriteFiles);
                }, cancellationToken);

                for (int i = 0; i < totalEntries; i++)
                {
                    BA2GeneralFileEntry entry = entries[i];
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
            finally
            {
                accessSemaphore.Release();
            }
        }

        private BA2GeneralFileEntry[] ConstructEntriesFromIndexes(IEnumerable<int> indexes)
        {
            BA2GeneralFileEntry[] entries = new BA2GeneralFileEntry[indexes.Count()];
            int i = 0;
            foreach (int index in indexes)
            {
                // TODO throw new IndexOutOfRange
                entries[i] = fileEntries[index];
                i++;
            }

            return entries;
        }

        #endregion

        #region Disposal

        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                fileEntries = null;
                if (accessSemaphore != null)
                    accessSemaphore.Dispose();
            }

            base.Dispose(disposeManagedResources);
        }

        #endregion
    }
}
