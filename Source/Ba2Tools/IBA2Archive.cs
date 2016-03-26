using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="destination">Absolute or relative directory path where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        void Extract(
            string fileName,
            string destination,
            bool overwriteFile = false);

        /// <summary>
        /// Extract single file to specified directory by index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="destination">Absolute or relative directory path where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        void Extract(
            int index,
            string destination,
            bool overwriteFile = false);

        /// <summary>
        /// Extract all files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to <c>fileNames.Count()</c>.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        void ExtractFiles(
            IEnumerable<string> fileNames,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false);

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token and progress reporter.
        /// </summary>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive's total files count.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        void ExtractAll(
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false);

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
        void ExtractFiles(
            IEnumerable<int> indexes,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress,
            bool overwriteFiles = false);

        /// <summary>
        /// Extract file, accessed by index, to the stream.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        bool ExtractToStream(int index, Stream stream);

        /// <summary>
        /// Extract file contents to stream.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        bool ExtractToStream(string fileName, Stream stream);

        /// <summary>
        /// Check file existance in archive.
        /// </summary>
        /// <param name="fileName">File to check in archive</param>
        /// <returns></returns>
        /// <remarks>
        /// Case-insensitive.
        /// </remarks>
        bool ContainsFile(string fileName);

        /// <summary>
        /// Gets the index from archive filename.
        /// </summary>
        /// <param name="fileName">Path to file in archive.</param>
        /// <returns>Index or -1 if not found.</returns>
        int GetIndexFromFilename(string fileName);

        /// <summary>
        /// Shows all file paths in archive.
        /// </summary>
        /// <param name="forceListFiles">Force list files in archive instead of returning cached copy.</param>
        /// <returns>
        /// List of file paths in archive.
        /// </returns>
        IList<string> ListFiles(bool forceListFiles = false);
    }
}
