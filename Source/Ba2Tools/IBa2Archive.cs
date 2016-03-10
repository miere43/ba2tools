using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Defines basic methods for all BA2 archives.
    /// </summary>
    public interface IBA2Archive
    {
        /// <summary>
        /// Extract single file to destination directory.
        /// </summary>
        /// <seealso cref="ListFiles(bool)"/>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="destination">Directory where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        void Extract(string fileName, string destination, bool overwriteFile = false);

        /// <summary>
        /// Extract all files from archive.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="destination">Directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        void ExtractFiles(string[] fileNames, string destination, bool overwriteFiles = false);

        /// <summary>
        /// Extract file contents to stream.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <returns>Success is true, failure is false.</returns>
        bool ExtractToStream(string fileName, Stream stream);

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <seealso cref="Extract(string, string, bool)"/>
        /// <param name="destination">Destination directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        void ExtractAll(string destination, bool overwriteFiles = false);

        /// <summary>
        /// Check file existance in archive.
        /// </summary>
        /// <param name="fileName">File to check in archive</param>
        /// <remarks>Case-insensitive</remarks>
        bool ContainsFile(string fileName);

        /// <summary>
        /// Shows all file paths in archive.
        /// </summary>
        /// <param name="forceListFiles">Force list files in archive instead of returning cached copy.</param>
        string[] ListFiles(bool forceListFiles = false);
    }
}
