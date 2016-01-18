using System.IO;

namespace Ba2ToolsTests
{
    /// <summary>
    /// Data shared between tests.
    /// </summary>
    public static class SharedData
    {
        /// <summary>
        /// Folder with test data.
        /// </summary>
        public static string DataFolder = "../../Data/";

        /// <summary>
        /// Folder to store temp data.
        /// </summary>
        public static string TempFolder = "../../Temp/";

        /// <summary>
        /// BA2 archive magic.
        /// </summary>
        public static byte[] ArchiveMagic = new byte[] { 0x42, 0x54, 0x44, 0x58 };

        /// <summary>
        /// Creates random temp folder in <c>TempFolder</c> and returns its path.
        /// </summary>
        /// <returns>Path to temp folder.</returns>
        public static string CreateTempDirectory()
        {
            string path = Path.Combine(TempFolder, Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Cleans temporary files and directories recursively.
        /// </summary>
        public static void CleanupTemp()
        {
            foreach (var dir in Directory.EnumerateDirectories(TempFolder))
            {
                Directory.Delete(dir, true);
            }
        }

        public static string GetDataPath(string item)
        {
            return Path.Combine(DataFolder, item);
        }
    }
}
