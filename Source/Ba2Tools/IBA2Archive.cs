using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;

namespace Ba2Tools
{
    /// <summary>
    /// Defines basic methods for all BA2 archives.
    /// </summary>
    internal interface IBA2Archive
    {
        /// <summary>
        /// File paths in archive.
        /// </summary>
        Dictionary<string, int>.KeyCollection FileList { get; }

        /// <summary>
        /// Extract single file to specified directory.
        /// </summary>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="destination">Absolute or relative directory path where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        void Extract(string fileName, string destination, bool overwriteFile);

        /// <summary>
        /// Extract single file to specified directory by index.
        /// </summary>
        /// <param name="fileIndex">File index.</param>
        /// <param name="destination">Absolute or relative directory path where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        void Extract(int fileIndex, string destination, bool overwriteFile);

        /// <summary>
        /// Extract all files from archive to specified directory with
        /// cancellation token and progress reporter.
        /// </summary>
        /// <param name="destination">Absolute or relative directory path directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to archive's total files count.</param>
        void ExtractAll(string destination, bool overwriteFiles, CancellationToken cancellationToken, IProgress<int> progress);

        /// <summary>
        /// Extract all files from archive to specified directory
        /// with cancellation token and progress reporter.
        /// </summary>
        /// <param name="fileNames">Files to extract.</param>
        /// <param name="destination">Absolute or relative directory path where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite existing files in extraction directory?</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Progress reporter ranged from 0 to <c>fileNames.Count()</c>.</param>
        void ExtractFiles(IEnumerable<string> fileNames, string destination, bool overwriteFiles, CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        /// Extracts specified files, accessed by index to the specified
        /// directory with cancellation token and progress reporter.
        /// </summary>
        /// <param name="fileIndexes">File indexes.</param>
        /// <param name="destination">
        /// Destination folder where extracted files will be placed.
        /// </param>
        /// <param name="overwriteFiles">Overwrite files in destination folder?</param>
        /// <param name="cancellationToken">
        /// The cancellation token. Set it to <c>CancellationToken.None</c>
        /// if you don't wanna cancel operation.
        /// </param>
        /// <param name="progress">
        /// Progress reporter ranged from 0 to <c>indexes.Count()</c>.
        /// Set it to <c>null</c> if you don't want to handle progress
        /// of operation.
        /// </param>
        void ExtractFiles(IEnumerable<int> fileIndexes, string destination, bool overwriteFiles, CancellationToken cancellationToken,
            IProgress<int> progress);

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
        /// Extract file, accessed by index, to the stream.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Success is true, failure is false.
        /// </returns>
        bool ExtractToStream(int index, Stream stream);

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
        int GetIndexFromFileName(string fileName);
    }
}
