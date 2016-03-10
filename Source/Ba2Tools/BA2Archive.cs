using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public UInt32 Version { get { return Header.Version; } }

        /// <summary>
        /// Number of files stored in archive.
        /// </summary>
        public UInt32 TotalFiles { get { return Header.TotalFiles; } }

        /// <summary>
        /// Offset to table where all filenames are listed.
        /// </summary>
        protected internal UInt64 NameTableOffset { get { return Header.NameTableOffset; } }

        /// <summary>
        /// Path to file that was opened.
        /// </summary>
        //public string FilePath { get; internal set; }

        public Stream ArchiveStream { get; internal set; }

        public BA2Header Header { get; internal set; }

        /// <summary>
        /// ListFiles() cache.
        /// </summary>
        protected List<string> _fileListCache = null;

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <seealso cref="ExtractFiles(IEnumerable{string}, string, bool)"/>
        /// <seealso cref="Extract(string, string, bool)"/>
        /// <param name="destination">Destination directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        public virtual void ExtractAll(string destination, bool overwriteFiles = false)
        {
            ExtractAll(destination, CancellationToken.None, null, overwriteFiles);
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
        public virtual void ExtractAll(string destination, CancellationToken cancellationToken, bool overwriteFiles = false)
        {
            this.ExtractAll(destination, cancellationToken, null, overwriteFiles);
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
        public virtual void ExtractAll(string destination, CancellationToken cancellationToken, IProgress<int> progress, bool overwriteFiles = false)
        {
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
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
        public virtual void ExtractFiles(IEnumerable<string> fileNames, string destination, CancellationToken cancellationToken, IProgress<int> progress, bool overwriteFiles = false)
        {
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public virtual void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles = false)
        {
            ExtractFiles(fileNames, destination, CancellationToken.None, null, overwriteFiles);
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public virtual void ExtractFiles(IEnumerable<string> fileNames, string destination, CancellationToken cancellationToken, bool overwriteFiles = false)
        {
            ExtractFiles(fileNames, destination, CancellationToken.None, null, overwriteFiles);
        }


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

        /// <summary>
        /// Lists all files in archive.
        /// </summary>
        /// <see cref="ExtractFile(string, string)"/>
        /// <param name="forceListFiles">Force list of files in archive instead of returning cached copy.</param>
        /// <returns>Array of file paths</returns>
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

        internal virtual void PreloadData(BinaryReader reader)
        {
            // No data to preload.
        }

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
            ArchiveStream.Dispose();
        }
    }
}
