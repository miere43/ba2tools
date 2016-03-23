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
    [TestClass()]
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
        /// Test name table, extraction methods.
        /// </summary>
        [TestMethod()]
        public void TestGeneralOneFile()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralOneFile.ba2"));
            var header = archive.Header;

            Assert.IsTrue(header.Signature.SequenceEqual(SharedData.ArchiveMagic));
            Assert.AreEqual(1U, header.Version);
            Assert.IsTrue(BA2Loader.GetArchiveType(header.ArchiveType) == BA2Type.General);
            Assert.AreEqual(1U, header.TotalFiles);
            Assert.AreEqual(69UL, header.NameTableOffset);

            var files = archive.ListFiles();
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual(true, archive.ContainsFile("test.txt"));

            var folder = SharedData.CreateTempDirectory();

            archive.Extract("test.txt", folder);
            Assert.IsTrue(File.Exists(Path.Combine(folder, "test.txt")));
            // Assert.IsTrue(File.ReadAllLines)
        }

        [TestMethod()]
        public void TestGeneralTwoFiles()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralTwoFiles.ba2"));
            var header = archive.Header;

            Assert.IsTrue(header.Signature.SequenceEqual(SharedData.ArchiveMagic));
            Assert.AreEqual(1U, header.Version);
            Assert.IsTrue(BA2Loader.GetArchiveType(header.ArchiveType) == BA2Type.General);
            Assert.AreEqual(2U, header.TotalFiles);
            Assert.AreEqual(121UL, header.NameTableOffset);

            var files = archive.ListFiles();
            Assert.AreEqual(2, files.Count);
            Assert.AreEqual("test.txt", files[0]);
            Assert.AreEqual("wazzup.bin", files[1]);

            var folder = SharedData.CreateTempDirectory();
            archive.Extract("test.txt", folder);

            var testPath = Path.Combine(folder, "test.txt");
            Assert.IsTrue(File.Exists(testPath));

            TestExtractedTestFile(archive, "test.txt", "test text");
            TestExtractedTestFile(archive, "wazzup.bin", "wazzup dude bro?");

            // Assert.IsTrue(File.ReadAllLines)
        }

        private void TestExtractedTestFile(BA2Archive archive, string fileName, string excepted)
        {
            using (var stream = new MemoryStream())
            {
                bool status = archive.ExtractToStream(fileName, stream);
                Assert.IsTrue(status);

                byte[] buffer = new byte[stream.Length];
                Assert.AreEqual(stream.Length, stream.Read(buffer, 0, (int)stream.Length));

                Assert.IsTrue(Encoding.ASCII.GetString(buffer).Equals(excepted, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// Test that header-only archives are valid.
        /// </summary>
        [TestMethod()]
        public void TestGeneralHeaderOnly()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnly.ba2"));
            var header = archive.Header;

            Assert.IsTrue(header.Signature.SequenceEqual(SharedData.ArchiveMagic));
            Assert.AreEqual(1U, header.Version);
            Assert.IsTrue(BA2Loader.GetArchiveType(header.ArchiveType) == BA2Type.General);
            Assert.AreEqual(0U, header.TotalFiles);

            var files = archive.ListFiles();
            Assert.AreEqual(0, files.Count);
            // Assert.AreEqual(69UL, header.NameTableOffset);
        }

        /// <summary>
        /// Test extraction to stream.
        /// </summary>
        [TestMethod()]
        public void TestStreamExtraction()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralOneFile.ba2"));
            using (var stream = new MemoryStream())
            {
                bool status = archive.ExtractToStream("test.txt", stream);
                Assert.IsTrue(status);

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);

                Assert.IsTrue(Encoding.ASCII.GetString(buffer).Equals("test text", StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Test to ensure general archive is threaten as general.
        /// </summary>
        [TestMethod()]
        public void TestGeneralArchiveType()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralOneFile.ba2"));
            Assert.IsInstanceOfType(archive, typeof(BA2GeneralArchive));
        }

        /// <summary>
        /// Test to ensure exception is thrown for non-archives
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(BA2LoadException))]
        public void TestInvalidArchive()
        {
            var archive = BA2Loader.Load(Path.Combine(SharedData.DataFolder, "InvalidArchive.txt"));
        }

        /// <summary>
        /// Test to ensures exception being thrown for invalid versions.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(BA2LoadException))]
        public void TestInvalidVersion()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnlyInvalidVersion.ba2"));
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid archive
        /// type when <c>LoadUnknownArchiveTypes</c> is set.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(BA2LoadException))]
        public void TestInvalidArchiveType()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnlyInvalidType.ba2"));
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid versions
        /// when <c>IgnoreVersion</c> flag is set in loader.
        /// </summary>
        [TestMethod()]
        public void TestKnownInvalidVersion()
        {
            var path = SharedData.GetDataPath("GeneralHeaderOnlyInvalidVersion.ba2");
            var archive = BA2Loader.Load(path, BA2LoaderFlags.IgnoreVersion);
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid archive
        /// type when <c>LoadUnknownArchiveTypes</c> is set.
        /// </summary>
        [TestMethod()]
        public void TestKnownInvalidArchiveType()
        {
            var path = SharedData.GetDataPath("GeneralHeaderOnlyInvalidType.ba2");
            var archive = BA2Loader.Load(path, BA2LoaderFlags.LoadUnknownArchiveTypes);
        }

        /// <summary>
        /// Test to ensure progress is being called properly.
        /// </summary>
        [TestMethod()]
        public void TestGeneralArchiveExtractionWithProgress()
        {
            var path = SharedData.GetDataPath("GeneralOneFile.ba2");
            var archive = BA2Loader.Load(path);
            var temp = SharedData.CreateTempDirectory();
            int progressValue = 0;
            bool progressReceived = false;
            var progressHandler = new Progress<int>(x =>
            {
                progressReceived = true;
                progressValue = x;
            });
            archive.ExtractAll(temp, CancellationToken.None, progressHandler);

            // workaround of dumb test execution
            int waits = 0;
            while (!progressReceived)
            {
                if (waits > 3)
                    break;
                Thread.Sleep(25);
                waits++;
            }
            Assert.AreEqual(1, progressValue);
        }
    }
}