using Ba2Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ba2ToolsTests
{
    [TestClass]
    public class GeneralArchiveTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            SharedData.CleanupTemp();
        }

        /// <summary>
        /// Test name table, extraction methods.
        /// </summary>
        [TestMethod]
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
            string path = Path.Combine(folder, "test.txt");

            Assert.IsTrue(File.Exists(path));
            Assert.AreEqual("test text", File.ReadAllText(path));
        }

        [TestMethod]
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

            TestUtils.AssertExtractedTextFile(archive, "test.txt", "test text");
            TestUtils.AssertExtractedTextFile(archive, "wazzup.bin", "wazzup dude bro?");

            // Assert.IsTrue(File.ReadAllLines)
        }

        /// <summary>
        /// Test that header-only archives are valid.
        /// </summary>
        [TestMethod]
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
        [TestMethod]
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
        [TestMethod]
        public void TestGeneralArchiveType()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralOneFile.ba2"));
            Assert.IsInstanceOfType(archive, typeof(BA2GeneralArchive));
        }

        /// <summary>
        /// Test to ensures exception being thrown for invalid versions.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BA2LoadException))]
        public void TestInvalidVersion()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnlyInvalidVersion.ba2"));
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid archive
        /// type when <c>LoadUnknownArchiveTypes</c> is set.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BA2LoadException))]
        public void TestInvalidArchiveType()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnlyInvalidType.ba2"));
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid versions
        /// when <c>IgnoreVersion</c> flag is set in loader.
        /// </summary>
        [TestMethod]
        public void TestKnownInvalidVersion()
        {
            var path = SharedData.GetDataPath("GeneralHeaderOnlyInvalidVersion.ba2");
            var archive = BA2Loader.Load(path, BA2LoaderFlags.IgnoreVersion);
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid archive
        /// type when <c>LoadUnknownArchiveTypes</c> is set.
        /// </summary>
        [TestMethod]
        public void TestKnownInvalidArchiveType()
        {
            var path = SharedData.GetDataPath("GeneralHeaderOnlyInvalidType.ba2");
            var archive = BA2Loader.Load(path, BA2LoaderFlags.LoadUnknownArchiveTypes);
        }

        /// <summary>
        /// Test to ensure progress is being called properly.
        /// </summary>
        [TestMethod]
        public void TestGeneralArchiveExtractionWithProgress()
        {
            var path = SharedData.GetDataPath("GeneralOneFile.ba2");

            BA2Archive archive = BA2Loader.Load(path);
            string temp = SharedData.CreateTempDirectory();
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

        [TestMethod]
        public void ExtractByIndexFromOneFileArchive()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralOneFileArchive);

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(archive.ExtractToStream(0, stream));
                TestUtils.AssertExtractedTextFile(stream, "test text");
            }

            archive.Dispose();
        }

        [TestMethod]
        public void ExtractByIndexFromTwoFileArchive()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive);

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(archive.ExtractToStream(0, stream));
                TestUtils.AssertExtractedTextFile(stream, "test text");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(archive.ExtractToStream(1, stream));
                TestUtils.AssertExtractedTextFile(stream, "wazzup dude bro?");
            }

            archive.Dispose();
        }

        [TestMethod]
        public void ExtractFilesByIndexesFromTwoFileArchive()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive);

            string temp = SharedData.CreateTempDirectory();
            archive.ExtractFiles(new int[] { 0, 1 }, temp, false);
            Assert.IsTrue(File.Exists(Path.Combine(temp, "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(temp, "wazzup.bin")));

            archive.Dispose();
        }

        [TestMethod]
        public void ExtractAllFromTwoFileArchive()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive);

            string temp = SharedData.CreateTempDirectory();
            archive.ExtractAll(temp, false);
            Assert.IsTrue(File.Exists(Path.Combine(temp, "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(temp, "wazzup.bin")));

            archive.Dispose();
        }

        [TestMethod]
        public void TestGenericArchiveLoader()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GetDataPath("GeneralOneFile.ba2"));
            archive.Dispose();
        }
    }
}
