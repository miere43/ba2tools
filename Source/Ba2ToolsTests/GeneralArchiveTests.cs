using Ba2Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework; 

namespace Ba2ToolsTests
{
    [TestFixture]
    public class GeneralArchiveTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            TestContext.WriteLine(TestContext.CurrentContext.WorkDirectory);
            TestContext.WriteLine(TestContext.CurrentContext.TestDirectory);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            SharedData.CleanupTemp();
        }

        /// <summary>
        /// Test name table, extraction methods.
        /// </summary>
        [Test]
        public void TestGeneralOneFile()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralOneFile.ba2"));
            var header = archive.Header;

            Assert.IsTrue(header.Signature.SequenceEqual(SharedData.ArchiveMagic));
            Assert.AreEqual(1U, header.Version);
            Assert.IsTrue(BA2Loader.GetArchiveType(header.ArchiveType) == BA2Type.General);
            Assert.AreEqual(1U, header.TotalFiles);
            Assert.AreEqual(69UL, header.NameTableOffset);

            var files = archive.FileList;
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual(true, archive.ContainsFile("test.txt"));

            var folder = SharedData.CreateTempDirectory();

            archive.Extract("test.txt", folder, false);
            string path = Path.Combine(folder, "test.txt");

            Assert.IsTrue(File.Exists(path));
            Assert.AreEqual("test text", File.ReadAllText(path));
        }

        [Test]
        public void TestGeneralTwoFiles()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralTwoFiles.ba2"));
            var header = archive.Header;

            Assert.IsTrue(header.Signature.SequenceEqual(SharedData.ArchiveMagic));
            Assert.AreEqual(1U, header.Version);
            Assert.IsTrue(BA2Loader.GetArchiveType(header.ArchiveType) == BA2Type.General);
            Assert.AreEqual(2U, header.TotalFiles);
            Assert.AreEqual(121UL, header.NameTableOffset);

            var files = archive.FileList;
            Assert.AreEqual(2, files.Count);
            Assert.AreEqual("test.txt", files[0]);
            Assert.AreEqual("wazzup.bin", files[1]);

            var folder = SharedData.CreateTempDirectory();
            archive.Extract("test.txt", folder, false);

            var testPath = Path.Combine(folder, "test.txt");
            Assert.IsTrue(File.Exists(testPath));

            TestUtils.AssertExtractedTextFile(archive, archive.GetIndexFromFilename("test.txt"), "test text");
            TestUtils.AssertExtractedTextFile(archive, archive.GetIndexFromFilename("wazzup.bin"), "wazzup dude bro?");

            // Assert.IsTrue(File.ReadAllLines)
        }

        [Test]
        public void InvalidDataExceptionThrownWhenInvalidNametableProviden()
        {
            Assert.Throws<InvalidDataException>(() => BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnly.ba2")));
        }

        /// <summary>
        /// Test extraction to stream.
        /// </summary>
        [Test]
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
        [Test]
        public void TestGeneralArchiveType()
        {
            var archive = BA2Loader.Load(SharedData.GetDataPath("GeneralOneFile.ba2"));
            Assert.IsInstanceOf<BA2GeneralArchive>(archive);
            Assert.IsTrue(BA2Loader.GetArchiveType(archive) == BA2Type.General);
        }

        /// <summary>
        /// Test to ensures exception being thrown for invalid versions.
        /// </summary>
        [Test]
        public void LoadExceptionThrownWhenArchiveHasInvalidVersion()
        {
            Assert.Throws<BA2LoadException>(() => BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnlyInvalidVersion.ba2")));
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid archive
        /// type when <c>LoadUnknownArchiveTypes</c> is not set.
        /// </summary>
        [Test]
        public void LoadExceptionThrownWhenArchiveHasInvalidType()
        {
            Assert.Throws<BA2LoadException>(() => BA2Loader.Load(SharedData.GetDataPath("GeneralHeaderOnlyInvalidType.ba2")));
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid versions
        /// when <c>IgnoreVersion</c> flag is set in loader.
        /// </summary>
        [Test]
        public void TestKnownInvalidVersion()
        {
            Assert.DoesNotThrow(
                () => BA2Loader.Load(SharedData.GetDataPath("GeneralOneFileInvalidVersion.ba2"), BA2LoaderFlags.IgnoreVersion));
        }

        /// <summary>
        /// Test to ensure no exception being thrown for invalid archive
        /// type when <c>LoadUnknownArchiveTypes</c> is set.
        /// </summary>
        [Test]
        public void TestKnownInvalidArchiveType()
        {
            Assert.DoesNotThrow(
                () => BA2Loader.Load(SharedData.GetDataPath("GeneralOneFileInvalidType.ba2"), BA2LoaderFlags.IgnoreArchiveType));
        }

        /// <summary>
        /// Test to ensure progress is being called properly.
        /// </summary>
        [Test]
        public void TestGeneralArchiveExtractionWithProgress()
        {
            BA2Archive archive = BA2Loader.Load(SharedData.GeneralOneFileArchive);
            string temp = SharedData.CreateTempDirectory();
            int progressValue = 0;
            bool progressReceived = false;
            var progressHandler = new Progress<int>(x =>
            {
                progressReceived = true;
                progressValue = x;
            });
            archive.ExtractAll(temp, false, CancellationToken.None, progressHandler);

            // workaround of dumb test execution
            int waits = 0;
            while (!progressReceived)
            {
                if (waits > 10)
                    break;
                Thread.Sleep(25);
                waits++;
            }
            Assert.AreEqual(1, progressValue);
        }

        [Test]
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

        [Test]
        public void ExtractByIndexFromOneFileArchiveMultithreaded()
        {
            BA2GeneralArchive archive
                = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralOneFileArchive, BA2LoaderFlags.Multithreaded);

            using (MemoryStream stream = new MemoryStream())
            {
                Assert.IsTrue(archive.ExtractToStream(0, stream));
                TestUtils.AssertExtractedTextFile(stream, "test text");
            }

            archive.Dispose();
        }

        [Test]
        public void ExtractByIndexFromTwoFileArchive()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive);

            TestUtils.AssertExtractedTextFile(archive, 0, "test text");
            TestUtils.AssertExtractedTextFile(archive, 1, "wazzup dude bro?");

            archive.Dispose();
        }

        [Test]
        public void ExtractByIndexFromTwoFileArchiveMultithreaded()
        {
            BA2GeneralArchive archive = 
                BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive, BA2LoaderFlags.Multithreaded);

            TestUtils.AssertExtractedTextFile(archive, 0, "test text");
            TestUtils.AssertExtractedTextFile(archive, 1, "wazzup dude bro?");

            archive.Dispose();
        }

        [Test]
        public void ExtractFilesByIndexesFromTwoFileArchive()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive);

            string temp = SharedData.CreateTempDirectory();
            archive.ExtractFiles(new int[] { 0, 1 }, temp, false);
            Assert.IsTrue(File.Exists(Path.Combine(temp, "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(temp, "wazzup.bin")));

            archive.Dispose();
        }

        [Test]
        public void ExtractFilesByIndexesFromTwoFileArchiveMultithreaded()
        {
            BA2GeneralArchive archive =
                BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive, BA2LoaderFlags.Multithreaded);

            string temp = SharedData.CreateTempDirectory();
            archive.ExtractFiles(new int[] { 0, 1 }, temp, false);
            Assert.IsTrue(File.Exists(Path.Combine(temp, "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(temp, "wazzup.bin")));

            archive.Dispose();
        }

        [Test]
        public void ExtractAllFromTwoFileArchive()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive);

            string temp = SharedData.CreateTempDirectory();
            archive.ExtractAll(temp, false);
            Assert.IsTrue(File.Exists(Path.Combine(temp, "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(temp, "wazzup.bin")));

            archive.Dispose();
        }

        [Test]
        public void ExtractAllFromTwoFilesArchiveMultithreaded()
        {
            BA2GeneralArchive archive = 
                BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralTwoFilesArchive, BA2LoaderFlags.Multithreaded);

            string temp = SharedData.CreateTempDirectory();
            archive.ExtractAll(temp, false);
            Assert.IsTrue(File.Exists(Path.Combine(temp, "test.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(temp, "wazzup.bin")));

            archive.Dispose();
        }

        [Test]
        public void TestGenericArchiveLoader()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralOneFileArchive);
            archive.Dispose();
        }

        [Test(Description = "Ensure that disposed archives throws ObjectDisposedException when extraction methods are accessed.")]
        public void ExtractShouldThrowExceptionWhenDisposed()
        {
            BA2GeneralArchive archive = BA2Loader.Load<BA2GeneralArchive>(SharedData.GeneralOneFileArchive);
            archive.Dispose();

            var dir = SharedData.CreateTempDirectory();

            using (var stream = new MemoryStream())
            {
                Assert.Throws<ObjectDisposedException>(() => archive.Extract(0, dir, false));
                Assert.Throws<ObjectDisposedException>(() => archive.Extract("test.txt", dir, false));
                Assert.Throws<ObjectDisposedException>(() => archive.ExtractToStream(0, stream));
                Assert.Throws<ObjectDisposedException>(() => archive.ExtractAll("", false));
                Assert.Throws<ObjectDisposedException>(() => archive.ExtractAll("", false, CancellationToken.None, null));
                Assert.Throws<ObjectDisposedException>(() => archive.ExtractFiles(new int[] { 0 }, dir, false));
                Assert.Throws<ObjectDisposedException>(() => archive.ExtractFiles(new int[] { 0 }, dir, false,
                    CancellationToken.None, null));
                Assert.Throws<ObjectDisposedException>(() => archive.ExtractFiles(new string[] { "test.txt" }, dir, false));
                Assert.Throws<ObjectDisposedException>(() => archive.ExtractFiles(new string[] { "test.txt" }, dir, false,
                    CancellationToken.None, null));

                // These methods should not throw ObjectDisposedException
                Assert.DoesNotThrow(() => {
                    var files = archive.FileList;
                    archive.GetIndexFromFilename("test.txt");
                    archive.GetIndexFromFilename("ajkkfajsdlkfjlkasdf");
                    var total = archive.TotalFiles;
                    var header = archive.Header;
                });
            }
        }
    }
}
