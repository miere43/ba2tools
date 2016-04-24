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
        /// Offset to table where all filenames are listed.
        /// </summary>
        protected internal UInt64 NameTableOffset { get { return Header.NameTableOffset; } }

        /// <summary>
        /// Gets the archive stream.
        /// </summary>
        /// <value>
        /// The archive stream.
        /// </value>
        public Stream ArchiveStream { get; internal set; }

        /// <summary>
        /// Gets the archive header.
        /// </summary>
        /// <value>
        /// The archive header.
        /// </value>
        public BA2Header Header { get; internal set; }

        public bool MultithreadedExtract { get; internal set; } = false;

        protected List<string> m_fileList = null;
        public IReadOnlyList<string> FileList { get { return m_fileList; } }

        #region Extract methods
        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <param name="destination">Destination directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public virtual void ExtractAll(string destination, bool overwriteFiles = false)
        {
            this.ExtractAll(destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token.
        /// </summary>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public virtual void ExtractAll(string destination, CancellationToken cancellationToken, bool overwriteFiles = false)
        {
            this.ExtractAll(destination, cancellationToken, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token and progress reporter.
        /// </summary>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive's total files count.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public virtual void ExtractAll(
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false)
        {
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public virtual void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles = false)
        {
            this.ExtractFiles(fileNames, destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public virtual void ExtractFiles(
            IEnumerable<string> fileNames,
            string destination,
            CancellationToken cancellationToken,
            bool overwriteFiles = false)
        {
            this.ExtractFiles(fileNames, destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extract specified files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to <c>fileNames.Count()</c>.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public virtual void ExtractFiles(
            IEnumerable<string> fileNames,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false)
        {
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
        public virtual void ExtractFiles(
            IEnumerable<int> indexes,
            string destination,
            bool overwriteFiles = false)
        {
            this.ExtractFiles(indexes, destination, CancellationToken.None, null, overwriteFiles);
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
        public virtual void ExtractFiles(
            IEnumerable<int> indexes,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false)
        {
            this.ExtractFiles((IEnumerable<string>)null, destination, cancellationToken, progress, overwriteFiles);
        }

        /// <summary>
        /// Extract file contents to stream.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        public virtual bool ExtractToStream(string fileName, Stream stream)
        {
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
        public virtual bool ExtractToStream(int index, Stream stream)
        {
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extract single file from archive.
        /// </summary>
        /// <param name="fileName">File path, directories separated with backslash (\)</param>
        /// <param name="destination">Destination directory where file will be extracted to.</param>
        /// <param name="overwriteFile">Overwrite existing file with extracted one?</param>
        public virtual void Extract(string fileName, string destination, bool overwriteFile = false)
        {
            throw new NotSupportedException("Cannot extract any files because archive type is not known.");
        }

        public virtual void Extract(int index, string destination, bool overwriteFile = false)
        {
            this.Extract(null, destination, overwriteFile);
        }

        #endregion

        /// <summary>
        /// Builds file list. NameTableOffset property must be set to valid value before calling this method. This method accesses
        /// archive stream and doesn't seek back to position before method call.
        /// </summary>
        /// <exception cref="System.IO.InvalidDataException" />
        protected virtual void BuildFileList()
        {
            if (this.m_fileList != null)
                return;

            // Not valid name table offset was given
            if (NameTableOffset < (UInt64)BA2Loader.HeaderSize)
                throw new InvalidDataException("Invalid name table offset was providen.");

            List<string> fileList = new List<string>((int)TotalFiles);

            ArchiveStream.Seek((long)NameTableOffset, SeekOrigin.Begin);
            using (var reader = new BinaryReader(ArchiveStream, Encoding.ASCII, leaveOpen: true))
            {
                long nameTableLength = ArchiveStream.Length - (long)NameTableOffset;
                if (nameTableLength < 1)
                    throw new InvalidDataException("Invalid name table offset was providen.");

                while (ArchiveStream.Length - ArchiveStream.Position >= 2)
                {
                    int remainingBytes = (int)(ArchiveStream.Length - ArchiveStream.Position);

                    UInt16 stringLength = reader.ReadUInt16();
                    byte[] rawstring = reader.ReadBytes(stringLength > remainingBytes ? remainingBytes : stringLength);

                    fileList.Add(Encoding.ASCII.GetString(rawstring));
                }
            }

            if (TotalFiles != fileList.Count)
                throw new InvalidDataException($"File list is not valid: excepted { TotalFiles } entries, but got { fileList.Count }");

            this.m_fileList = fileList;
        }

        /// <summary>
        /// Preloads the data.
        /// </summary>
        /// <param name="reader">The reader.</param>
        internal virtual void PreloadData(BinaryReader reader)
        {
            BuildFileList();
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
            if (m_fileList == null)
                BuildFileList();

            return m_fileList.Contains(fileName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the index from archive filename.
        /// </summary>
        /// <param name="fileName">Path to file in archive.</param>
        /// <returns>Index or -1 if not found.</returns>
        public virtual int GetIndexFromFilename(string fileName)
        {
            int length = m_fileList.Count;
            for (int i = 0; i < length; i++)
            {
                if (m_fileList[i].Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        #region Helper methods

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

            if (m_fileList == null)
                BuildFileList();

            for (int i = 0; i < entriesLength; i++)
            {
                entry = entries[i];

                string path = CreateDirectoryAndGetPath(entry, destination, overwriteFiles);
                coll.Add(path, cancel);
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
            string extractPath = Path.Combine(destination, m_fileList[entry.Index]);

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
                int index = GetIndexFromFilename(name);
                if (index == -1)
                    throw new BA2ExtractionException($"File \"{name}\" is not found in archive");

                indexes[i] = index;
                i++;
            }

            return indexes;
        }

        #endregion

        ~BA2Archive()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes BA2Archive instance and frees its resources.
        /// After this call BA2Archive is not usable anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ArchiveStream != null)
                {
                    ArchiveStream.Dispose();
                    ArchiveStream = null;
                }
            }
        }
    }
}
