using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ba2Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ba2Tools.Tests
{
    public static class SharedData
    {
        public static string DataFolder = "../../Data/";

        public static string TempFolder = "../../Temp/";

        public static byte[] ArchiveMagic = new byte[] { 0x42, 0x54, 0x44, 0x58 };

        public static byte[] GeneralArchiveTypeMagic = new byte[] { 0x47, 0x4E, 0x52, 0x4C };

        public static string CreateTempDirectory()
        {
            string path = Path.Combine(TempFolder, Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        }

        public static void CleanupTemp()
        {
            foreach (var dir in Directory.EnumerateDirectories(TempFolder))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    [TestClass()]
    public class Ba2ArchiveLoader_Tests
    {
        [TestCleanup]
        public void Cleanup()
        {
            SharedData.CleanupTemp();
        }

        [TestMethod()]
        public void TestGeneralOneFile()
        {
            var archive = BA2Loader.Load(Path.Combine(SharedData.DataFolder, "GeneralOneFile.ba2"));
            var header = archive.Header;

            Assert.IsTrue(header.Signature.SequenceEqual(SharedData.ArchiveMagic));
            Assert.AreEqual(1U, header.Version);
            Assert.IsTrue(header.ArchiveType.SequenceEqual(SharedData.GeneralArchiveTypeMagic));
            Assert.AreEqual(1U, header.TotalFiles);
            Assert.AreEqual(69UL, header.NameTableOffset);

            string[] files = archive.ListFiles();
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(true, archive.ContainsFile("test.txt"));

            var folder = SharedData.CreateTempDirectory();

            archive.Extract("test.txt", folder);
            Assert.IsTrue(File.Exists(Path.Combine(folder, "test.txt")));
            // Assert.IsTrue(File.ReadAllLines)
        }

        [TestMethod()]
        public void TestGeneralHeaderOnly()
        {
            var archive = BA2Loader.Load(Path.Combine(SharedData.DataFolder, "GeneralHeaderOnly.ba2"));
            var header = archive.Header;

            Assert.IsTrue(header.Signature.SequenceEqual(SharedData.ArchiveMagic));
            Assert.AreEqual(1U, header.Version);
            Assert.IsTrue(header.ArchiveType.SequenceEqual(SharedData.GeneralArchiveTypeMagic));
            Assert.AreEqual(0U, header.TotalFiles);

            var files = archive.ListFiles();
            Assert.AreEqual(0, files.Length);
            // Assert.AreEqual(69UL, header.NameTableOffset);
        }

        [TestMethod()]
        public void TestStreamExtraction()
        {
            var archive = BA2Loader.Load(Path.Combine(SharedData.DataFolder, "GeneralOneFile.ba2"));
            using (var stream = new MemoryStream())
            {
                bool status = archive.ExtractToStream("test.txt", stream);
                Assert.IsTrue(status);

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);

                Assert.IsTrue(Encoding.ASCII.GetString(buffer).Equals("test text", StringComparison.OrdinalIgnoreCase));
            }
                
        }
    }
}