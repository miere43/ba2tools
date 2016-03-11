using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Defines basic methods for all BA2 archives.
    /// </summary>
    public interface IBA2Archive
    {
        /// <summary>
        /// Extract single file to specified directory.
        /// </summary>
        /// <seealso cref="ListFiles(bool)"/>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="destination">Absolute or relative directory path where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        void Extract(string fileName, string destination, bool overwriteFile = false);

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles = false);

        /// <summary>
        /// Extract all files from archive to destination directory
        /// with cancellation token.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        void ExtractFiles(IEnumerable<string> fileNames, string destination, CancellationToken cancellationToken, bool overwriteFiles = false);

        /// <summary>
        /// Extract all files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <seealso cref="BA2ExtractionException"/>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to <c>fileNames.Count()</c>.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        void ExtractFiles(IEnumerable<string> fileNames, string destination, CancellationToken cancellationToken, IProgress<int> progress, bool overwriteFiles = false);

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <seealso cref="ExtractFiles(IEnumerable{string}, string, bool)"/>
        /// <seealso cref="Extract(string, string, bool)"/>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        void ExtractAll(string destination, bool overwriteFiles = false);

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token.
        /// </summary>
        /// <seealso cref="ExtractFiles(IEnumerable{string}, string, bool)"/>
        /// <seealso cref="Extract(string, string, bool)"/>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        void ExtractAll(string destination, CancellationToken cancellationToken, bool overwriteFiles = false);

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
        void ExtractAll(string destination, CancellationToken cancellationToken, IProgress<int> progress, bool overwriteFiles = false);

        /// <summary>
        /// Extract file contents to stream.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <returns>Success is true, failure is false.</returns>
        bool ExtractToStream(string fileName, Stream stream);

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
        IList<string> ListFiles(bool forceListFiles = false);
    }
}
