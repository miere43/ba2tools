using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ba2Tools;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Ba2ToolsTests
{
    /// <summary>
    /// Tests for BA2Tools library.
    /// </summary>
    [TestClass]
    public class BA2ArchiveLoaderTests
    {
        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            SharedData.CleanupTemp();
        }

        /// <summary>
        /// Test to ensure exception is thrown for non-archives
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BA2LoadException))]
        public void TestInvalidArchive()
        {
            var archive = BA2Loader.Load(Path.Combine(SharedData.DataFolder, "InvalidArchive.txt"));
        }

        /// <summary>
        /// Test to ensure exception is thrown for generic methods.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BA2LoadException))]
        public void TestInvalidGenericArchiveLoad()
        {
            BA2TextureArchive archive = BA2Loader.Load<BA2TextureArchive>(SharedData.GetDataPath("GeneralOneFile.ba2"));
        }
    }
}