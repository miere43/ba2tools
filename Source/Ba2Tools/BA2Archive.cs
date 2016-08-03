using Ba2Tools.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;

namespace Ba2Tools
{
    /// <summary>
    /// Represents base BA2 archive type. Contains everything that other archive types contain.
    /// </summary>
    public class BA2Archive : IBA2Archive, IDisposable
    {
        protected bool m_disposed;

        protected Dictionary<string, int> m_fileNames;

        internal Stream m_archiveStream;

        #region Public Properties

        /// <summary>
        /// Archive version defined in header.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public UInt32 Version { get { return Header.Version; } }

        /// <summary>
        /// Number of files stored in archive.
        /// </summary>
        /// <value>
        /// The total files.
        /// </value>
        public UInt32 TotalFiles { get { return Header.TotalFiles; } }

        /// <summary>
        /// Get underlying archive stream length in bytes.
        /// </summary>
        public Int64 Length { get { return m_archiveStream.Length; } }

        /// <summary>
        /// Gets the archive header.
        /// </summary>
        /// <value>
        /// The archive header.
        /// </value>
        public BA2Header Header { get; internal set; }

        /// <summary>
        /// Is multithreaded extraction enabled?
        /// </summary>
        public bool IsMultithreaded { get; internal set; } = false;

        /// <summary>
        /// List of file names that mapped to their index in archive.
        /// </summary>
        /// <seealso cref="GetFileIndex(string)"/>
        public Dictionary<string, int>.KeyCollection FileList { get { return m_fileNames.Keys; } }

        #endregion

        #region IBA2Archive Implementation
        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <param name="destination">Destination directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void ExtractAll(string destination, bool overwriteFiles)
        {
            CheckDisposed();
            this.ExtractAll(destination, overwriteFiles, CancellationToken.None, null);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token.
        /// </summary>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void ExtractAll(string destination, bool overwriteFiles, CancellationToken cancellationToken)
        {
            CheckDisposed();
            this.ExtractAll(destination, overwriteFiles, cancellationToken, null);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token and progress reporter.
        /// </summary>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive's total files count.</param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void ExtractAll(string destination, bool overwriteFiles, CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            CheckDisposed();
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles)
        {
            CheckDisposed();
            this.ExtractFiles(fileNames, destination, overwriteFiles, CancellationToken.None, null);
        }

        /// <summary>
        /// Extract specified files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to <c>fileNames.Count()</c>.</param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles,
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            CheckDisposed();
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
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
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void ExtractFiles(IEnumerable<int> indexes, string destination, bool overwriteFiles)
        {
            CheckDisposed();
            this.ExtractFiles(indexes, destination, overwriteFiles, CancellationToken.None, null);
        }

        /// <summary>
        /// Extracts specified files, accessed by index to the specified
        /// directory with cancellation token and progress reporter.
        /// </summary>
        /// <param name="indexes">The indexes.</param>
        /// <param name="destination">
        /// Destination folder where extracted files will be placed.
        /// </param>
        /// <param name="overwriteFiles">Overwrite files in destination folder?</param>
        /// <param name="cancellationToken">
        /// The cancellation token. Set it to <c>CancellationToken.None</c>
        /// if you don't want to cancel operation.
        /// </param>
        /// <param name="progress">
        /// Progress reporter ranged from 0 to <c>indexes.Count()</c>.
        /// Set it to <c>null</c> if you don't want to handle progress
        /// of operation.
        /// </param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void ExtractFiles(IEnumerable<int> indexes, string destination, bool overwriteFiles,
            CancellationToken cancellationToken, IProgress<int> progress)
        {
            CheckDisposed();
            this.ExtractFiles((IEnumerable<string>)null, destination, overwriteFiles, cancellationToken, progress);
        }

        /// <summary>
        /// Extract file contents to stream.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual bool ExtractToStream(string fileName, Stream stream)
        {
            CheckDisposed();
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extract file, accessed by index, to the stream.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual bool ExtractToStream(int index, Stream stream)
        {
            CheckDisposed();
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extract single file from archive.
        /// </summary>
        /// <param name="fileName">File path, directories separated with backslash (\)</param>
        /// <param name="destination">Destination directory where file will be extracted.</param>
        /// <param name="overwriteFile">Overwrite existing file with extracted one?</param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void Extract(string fileName, string destination, bool overwriteFile)
        {
            CheckDisposed();
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extracts file from archive, accessed by index.
        /// </summary>
        /// <param name="fileIndex">File index. See <c>GetFileIndex()</c>.</param>
        /// <param name="destination">Destination directory where file will be extracted.</param>
        /// <param name="overwriteFile">Overwrite existing file with extracted one?</param>
        /// <exception cref="System.ObjectDisposedException" />
        public virtual void Extract(int fileIndex, string destination, bool overwriteFile)
        {
            CheckDisposed();
            this.Extract(null, destination, overwriteFile);
        }

        /// <summary>
        /// Check file existance in archive.
        /// </summary>
        /// <param name="fileName">File to check in archive</param>
        /// <returns></returns>
        /// <remarks>
        /// Case-insensitive.
        /// </remarks>
        public virtual bool ContainsFile(string fileName)
        {
            return m_fileNames.ContainsKey(fileName);
        }

        /// <summary>
        /// Preloads the data.
        /// </summary>
        /// <param name="reader">The reader.</param>
        internal virtual void PreloadData(BinaryReader reader)
        {
            CheckDisposed();
            BuildFileList();
        }

        #endregion

        #region Helper methods

        public virtual UInt32 GetFileSize(int fileIndex)
        {
            CheckDisposed();
            throw new NotSupportedException("Cannot get file size because archive type is unknown.");
        }

        /// <summary>
        /// Gets the index from archive filename.
        /// </summary>
        /// <param name="fileName">Path to file in archive.</param>
        /// <returns>Index or -1 if not found.</returns>
        public virtual int GetFileIndex(string fileName)
        {
            int index;
            return m_fileNames.TryGetValue(fileName, out index) ? index : -1;
        }

        /// <summary>
        /// Builds file list. NameTableOffset property must be set to valid value before calling this method. This method accesses
        /// archive stream and doesn't seek back to position before method call.
        /// </summary>
        /// <exception cref="System.IO.InvalidDataException" />
        protected virtual void BuildFileList()
        {
            if (this.m_fileNames != null)
                return;

            // Not valid name table offset was given
            if (Header.NameTableOffset < (UInt64)BA2Loader.HeaderSize)
                throw new InvalidDataException("Invalid name table offset was providen.");

            m_fileNames = new Dictionary<string, int>((int)TotalFiles, StringComparer.OrdinalIgnoreCase);

            m_archiveStream.Seek((long)Header.NameTableOffset, SeekOrigin.Begin);
            using (var reader = new BinaryReader(m_archiveStream, Encoding.ASCII, leaveOpen: true))
            {
                long nameTableLength = m_archiveStream.Length - (long)Header.NameTableOffset;
                if (nameTableLength < 1)
                    throw new InvalidDataException("Invalid name table offset was providen.");

                int i = 0;
                while (m_archiveStream.Length - m_archiveStream.Position >= 2)
                {
                    if (i >= TotalFiles)
                        throw new InvalidDataException($"File list is not valid: excepted { TotalFiles } entries, but got { i + 1 }");
                    int remainingBytes = (int)(m_archiveStream.Length - m_archiveStream.Position);

                    UInt16 stringLength = reader.ReadUInt16();
                    byte[] rawstring = reader.ReadBytes(stringLength > remainingBytes ? remainingBytes : stringLength);

                    m_fileNames[Encoding.ASCII.GetString(rawstring)] = i;
                    ++i;
                }

                if (TotalFiles != i)
                    throw new InvalidDataException($"File list is not valid: excepted { TotalFiles } entries, but got { i + 1 }");
            }
        }

        /// <summary>
        /// Creates the directories for files.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <param name="coll">The coll.</param>
        /// <param name="cancel">The cancel.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="overwriteFiles">if set to <c>true</c> [overwrite files].</param>
        protected void CreateDirectoriesForFiles(
            IBA2FileEntry[] entries,
            BlockingCollection<string> coll,
            CancellationToken cancel,
            string destination,
            bool overwriteFiles)
        {
            IBA2FileEntry entry;
            int entriesLength = entries.Length;
            int checkEach = entriesLength / 5;
            int check = checkEach;

            if (m_fileNames == null)
                BuildFileList();

            for (int i = 0; i < entriesLength; i++)
            {
                entry = entries[i];

                string path = CreateDirectoryAndGetPath(entry, destination, overwriteFiles);
                coll.Add(path, cancel);

                if (i == check)
                {
                    cancel.ThrowIfCancellationRequested();
                    check += checkEach;
                }
            }
        }

        /// <summary>
        /// Creates the directory and get path.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="overwriteFile">if set to <c>true</c> [overwrite file].</param>
        /// <returns></returns>
        /// <exception cref="BA2ExtractionException"></exception>
        protected string CreateDirectoryAndGetPath(IBA2FileEntry entry, string destination, bool overwriteFile)
        {
            string extension = new string(entry.Extension).Trim('\0');
            string extractPath = Path.Combine(destination, m_fileNames.Keys.ElementAt(entry.Index));

            if (overwriteFile == false && File.Exists(extractPath))
                throw new BA2ExtractionException($"File \"{ extractPath }\" already exists and overwrite is not permitted.");

            string extractFolder = Path.GetDirectoryName(extractPath);
            Directory.CreateDirectory(extractFolder);

            return extractPath;
        }

        /// <summary>
        /// Gets the indexes from filenames.
        /// </summary>
        /// <param name="fileNames">The file names.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="BA2ExtractionException"></exception>
        protected int[] GetIndexesFromFilenames(IEnumerable<string> fileNames)
        {
            if (fileNames == null)
                throw new ArgumentNullException(nameof(fileNames));

            int[] indexes = new int[fileNames.Count()];

            int i = 0;
            foreach (string name in fileNames)
            {
                int index = GetFileIndex(name);
                if (index == -1)
                    throw new BA2ExtractionException($"File \"{name}\" is not found in archive");

                indexes[i] = index;
                i++;
            }

            return indexes;
        }

        #endregion

        #region Disposal

        ~BA2Archive()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes BA2Archive instance and frees its resources. After this call extraction methods of BA2Archive are not usable anymore.
        /// </summary>
        public virtual void Dispose()
        {
            if (m_disposed) return;

            if (m_archiveStream != null)
            {
                m_archiveStream.Dispose();
                m_archiveStream = null;
            }

            m_disposed = true;
            GC.SuppressFinalize(this);
        }

        protected void CheckDisposed()
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(Stream));
        }

        #endregion
    }
}
