using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Defines basic methods for all BA2 archives.
    /// </summary>
    public interface IBa2Archive
    {
        /// <summary>
        /// Extract single file to destination directory.
        /// </summary>
        /// <seealso cref="ListFiles(bool)"/>
        /// <param name="fileName">File name or file path from archive.</param>
        /// <param name="destination">Directory where extracted file will be placed.</param>
        /// <param name="overwriteFile">Overwrite existing file in directory with extracted one?</param>
        void Extract(string fileName, string destination, bool overwriteFile = false);

        // byte[] GetFileData(string fileName);

        /// <summary>
        /// Extract all files from archive to specified directory.
        /// </summary>
        /// <seealso cref="Extract(string, string, bool)"/>
        /// <param name="destination">Destination directory where extracted files will be placed.</param>
        /// <param name="overwriteFiles">Overwrite files on disk with extracted ones?</param>
        void ExtractAll(string destination, bool overwriteFiles = false);

        /// <summary>
        /// Shows all file paths in archive.
        /// </summary>
        /// <param name="forceListFiles">Force list files in archive instead of returning cached copy.</param>
        /// <returns></returns>
        string[] ListFiles(bool forceListFiles = false);
    }
}
