using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Represents base BA2 archive type. Contains everything that other archive types contain.
    /// </summary>
    public class Ba2ArchiveBase : IBa2Archive
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
        public string FilePath { get; internal set; }

        public Ba2ArchiveLoader.Ba2ArchiveHeader Header { get; internal set; }

        /// <summary>
        /// ListFiles() cache.
        /// </summary>
        protected string[] _fileListCache = null;

        public virtual void ExtractAll(string destination, bool overwriteFiles = false)
        {
            throw new NotSupportedException("Cannot extract any files because archive type is unknown.");
        }

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <seealso cref="Ba2ArchiveExtractionException"/>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        public virtual void ExtractFiles(string[] fileNames, string destination, bool overwriteFiles = false)
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
        public virtual string[] ListFiles(bool forceListFiles = false)
        {
            if (_fileListCache != null && forceListFiles == false)
                return _fileListCache;

            List<string> strings = new List<string>();

            using (var fileStream = File.OpenRead(FilePath)) {
                using (var reader = new BinaryReader(fileStream, Encoding.ASCII)) {
                    long nameTableLength = fileStream.Length - fileStream.Position;

                    fileStream.Seek((long)NameTableOffset, SeekOrigin.Begin);
                    fileStream.Lock((long)NameTableOffset, nameTableLength);

                    while (fileStream.Length - fileStream.Position >= 2)
                    {
                        int remainingBytes = (int)(fileStream.Length - fileStream.Position);

                        UInt16 stringLength = reader.ReadUInt16();
                        byte[] rawstring = reader.ReadBytes(stringLength > remainingBytes ? remainingBytes : stringLength);

                        strings.Add(Encoding.ASCII.GetString(rawstring));
                    }

                    fileStream.Unlock((long)NameTableOffset, nameTableLength);
                }
            }

            // Is all files in archive were listed? (excepted "FileCount" files)
            System.Diagnostics.Debug.Assert(TotalFiles == strings.Count);

            _fileListCache = strings.ToArray();
            return _fileListCache;
        }

        internal virtual void PreloadData(BinaryReader reader)
        {

        }

        public virtual bool ContainsFile(string fileName)
        {
            if (_fileListCache == null)
                ListFiles();

            for (int i = 0; i < _fileListCache.Length; i++)
            {
                if (fileName.Equals(_fileListCache[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }


    }
}
