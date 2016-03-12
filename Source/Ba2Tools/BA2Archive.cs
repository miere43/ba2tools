using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

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

        /// <summary>
        /// ListFiles() cache.
        /// </summary>
        /// <seealso cref="ListFiles(bool)"/>
        protected List<string> _fileListCache = null;

        ~BA2Archive()
        {
            Dispose(false);
        }

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
        /// Extract all files from archive to specified directory
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
        /// Extract single file from archive.
        /// </summary>
        /// <param name="fileName">File path, directories separated with backslash (\)</param>
        /// <param name="destination">Destination directory where file will be extracted to.</param>
        /// <param name="overwriteFile">Overwrite existing file with extracted one?</param>
        public virtual void Extract(string fileName, string destination, bool overwriteFile = false)
        {
            throw new NotSupportedException("Cannot extract any files because archive type is not known.");
        }
        #endregion

        /// <summary>
        /// Shows all file paths in archive.
        /// </summary>
        /// <param name="forceListFiles">Force list files in archive instead of returning cached copy.</param>
        /// <returns>
        /// List of file paths in archive.
        /// </returns>
        public virtual IList<string> ListFiles(bool forceListFiles = false)
        {
            if (_fileListCache != null && forceListFiles == false)
                return _fileListCache;

            // Not valid name table offset was given
            if (NameTableOffset < (UInt64)BA2Loader.HeaderSize)
            {
                goto invalidNameTableProviden;
            }

            List<string> strings = new List<string>(Math.Min(10000, (int)TotalFiles));

            ArchiveStream.Seek((long)NameTableOffset, SeekOrigin.Begin);
            using (var reader = new BinaryReader(ArchiveStream, Encoding.ASCII, leaveOpen: true))
            {
                long nameTableLength = ArchiveStream.Length - (long)NameTableOffset;
                if (nameTableLength < 0)
                    goto invalidNameTableProviden;

                while (ArchiveStream.Length - ArchiveStream.Position >= 2)
                {
                    int remainingBytes = (int)(ArchiveStream.Length - ArchiveStream.Position);

                    UInt16 stringLength = reader.ReadUInt16();
                    byte[] rawstring = reader.ReadBytes(stringLength > remainingBytes ? remainingBytes : stringLength);

                    strings.Add(Encoding.ASCII.GetString(rawstring));
                }
            }

            // Is all files in archive were listed? (excepted "FileCount" files)
            System.Diagnostics.Debug.Assert(TotalFiles == strings.Count);

            _fileListCache = strings;
            return _fileListCache;

        /// goto case when invalid name table offset was providen
        invalidNameTableProviden:
            {
                _fileListCache = new List<string>(0);
                return _fileListCache;
            }
        }

        /// <summary>
        /// Preloads the data.
        /// </summary>
        /// <param name="reader">The reader.</param>
        internal virtual void PreloadData(BinaryReader reader)
        {
            // No data to preload.
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
            if (_fileListCache == null)
                ListFiles();

            return _fileListCache.Contains(fileName, StringComparer.OrdinalIgnoreCase);
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
