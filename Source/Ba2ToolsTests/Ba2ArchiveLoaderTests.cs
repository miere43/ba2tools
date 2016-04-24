using Ba2Tools;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Ba2ToolsTests
{
    /// <summary>
    /// Tests for BA2Tools library.
    /// </summary>
    [TestFixture]
    public class BA2ArchiveLoaderTests
    {
        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [OneTimeTearDown]
        public void Cleanup()
        {
            SharedData.CleanupTemp();
        }

        /// <summary>
        /// Test to ensure exception is thrown for non-archives
        /// </summary>
        [Test]
        public void TestInvalidArchive()
        {
            Assert.Throws<BA2LoadException>(() => BA2Loader.Load(Path.Combine(SharedData.DataFolder, "InvalidArchive.txt")));
        }

        /// <summary>
        /// Test to ensure exception is thrown for generic methods.
        /// </summary>
        [Test]
        public void TestInvalidGenericArchiveLoad()
        {
            Assert.Throws<BA2LoadException>(() => BA2Loader.Load<BA2TextureArchive>(SharedData.GeneralOneFileArchive));
        }
    }
}