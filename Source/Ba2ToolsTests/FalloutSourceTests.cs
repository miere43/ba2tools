using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ba2Tools;
using Microsoft.Win32;
using NUnit.Framework;

namespace Ba2ToolsTests
{
    [TestFixture(Description = "Fallout 4 .ba2 archives required to run these tests.")]
    [Category("Fallout4SourceTests")]
    [Category("LongTests")]
    public class FalloutSourceTests
    {
        private static string m_installPath = "";

        private static object m_lock = new object();

        /// <summary>
        /// Rethieves Fallout 4 installation path from registry.
        /// </summary>
        /// <returns>Fallout 4 installation path or null if not found.</returns>
        public static bool DiscoverFallout4InstallPath()
        {
            lock (m_lock)
            {
                if (m_installPath != "")
                    return true;

                RegistryKey fonode;
                if (Environment.Is64BitOperatingSystem)
                    fonode = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\bethesda softworks\Fallout4", false);
                else
                    fonode = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\bethesda softworks\Fallout4", false);

                if (fonode == null) return false;

                object installedPath = fonode.GetValue("Installed Path");
                m_installPath = installedPath as string;
                if (!m_installPath.EndsWith("\\"))
                    m_installPath = m_installPath + '\\';
                return true;
            }
        }

        private static string GetArchivePath(string path)
        {
            if (m_installPath == "") return "";
            return Path.Combine(m_installPath, @"Data\", path);
        }

        [OneTimeSetUp]
        public void CanRunTests()
        {
            if (!DiscoverFallout4InstallPath())
                Assert.Ignore("Fallout 4 install path was not found. Skipping Fallout4Source tests.");
        }

        [Test, TestCaseSource(nameof(archives))]
        [Parallelizable(ParallelScope.Children)]
        public void FalloutArchivesExtractTest(string archiveName, byte[] signature, uint version, byte[] type, uint totalFiles, ulong nameTableOffset)
        {
            var archivePath = GetArchivePath(archiveName);
            if (!File.Exists(archivePath))
                Assert.Ignore("Archive {0} was not found. Skipping test.", archivePath);

            var header = BA2Loader.LoadHeader(archivePath);
            Assert.AreEqual(header.Signature, signature, "Signatures don't match.");
            Assert.AreEqual(header.Version, version, "Versions don't match.");
            Assert.AreEqual(header.ArchiveType, type, "Types don't match.");
            Assert.AreEqual(header.TotalFiles, totalFiles, "Total files don't match.");
            Assert.AreEqual(header.NameTableOffset, nameTableOffset, "Table offset don't match.");

            using (var archive = BA2Loader.Load(archivePath, BA2LoaderFlags.None))
            {
                using (var memory = new MemoryStream())
                {
                    Assert.IsTrue(archive.ExtractToStream(0, memory));
                }
            }
        }

        [Test(Description = "Check that it is safe to extract files from archive from several threads at once.")]
        public void ExtractShouldWorkWithSeveralThreads()
        {
            BA2TextureArchive archive = BA2Loader.Load<BA2TextureArchive>(GetArchivePath("Fallout4 - Textures1.ba2"));
            Assert.Greater(archive.TotalFiles, 100, "Archive should have at least 100 files in it to check for multiple threads access.");
            Parallel.For(0, Math.Min(100, archive.TotalFiles), (fileIndex) =>
            {
                using (var stream = new MemoryStream())
                {
                    // overlapping threads will corrupt archive stream if no locks in there.
                    archive.ExtractToStream((int)fileIndex, stream);
                }
            });
        }

        private void GetFileSizeTest(int fileIndex, BA2Archive a)
        {
            var exceptedLen = a.GetFileSize(fileIndex);
            var actualLen = 0u;
            using (var stream = new MemoryStream())
            {
                a.ExtractToStream(fileIndex, stream);
                actualLen = (uint)stream.Length;

                Assert.AreEqual(exceptedLen, actualLen);
            }
        }

        [Test]
        public void GeneralArchiveGetFileSizeTest()
        {
            using (var a = BA2Loader.Load<BA2GeneralArchive>(GetArchivePath("Fallout4 - Sounds.ba2")))
                GetFileSizeTest(0, a);
        }

        [Test]
        public void TextureArchiveGetFileSizeTest()
        {
            using (var a = BA2Loader.Load<BA2TextureArchive>(GetArchivePath("Fallout4 - Textures1.ba2")))
                GetFileSizeTest(0, a);
        }

        #region Archive Info

        // Auto-generated Wednesday, 03 August 2016 (UTC)
        // filePath, signature, version, type, totalFiles, nameTableOffset
        private static object[] archives = {
            new object[] { "DLCRobot - Main.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 6050U, 362911874UL },
            new object[] { "DLCRobot - Textures.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 353U, 218548977UL },
            new object[] { "DLCRobot - Voices_en.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 6112U, 122546078UL },
            new object[] { "DLCworkshop01 - Main.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 1263U, 55379162UL },
            new object[] { "DLCworkshop01 - Textures.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 32U, 23232616UL },
            new object[] { "Fallout4 - Animations.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 29706U, 368586410UL },
            new object[] { "Fallout4 - Interface.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 445U, 24440610UL },
            new object[] { "Fallout4 - Materials.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 6887U, 1436230UL },
            new object[] { "Fallout4 - Meshes.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 42356U, 1481910396UL },
            new object[] { "Fallout4 - MeshesExtra.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 125837U, 1372108897UL },
            new object[] { "Fallout4 - Misc.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 7887U, 11640566UL },
            new object[] { "Fallout4 - Nvflex.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 32U, 6383680UL },
            new object[] { "Fallout4 - Shaders.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 1U, 12561088UL },
            new object[] { "Fallout4 - Sounds.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 8872U, 1579167914UL },
            new object[] { "Fallout4 - Startup.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 51U, 19468216UL },
            new object[] { "Fallout4 - Textures1.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 2703418692UL },
            new object[] { "Fallout4 - Textures2.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 2410440805UL },
            new object[] { "Fallout4 - Textures3.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 2100879994UL },
            new object[] { "Fallout4 - Textures4.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 1999775741UL },
            new object[] { "Fallout4 - Textures5.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 1963549908UL },
            new object[] { "Fallout4 - Textures6.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 1382669557UL },
            new object[] { "Fallout4 - Textures7.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 545751391UL },
            new object[] { "Fallout4 - Textures8.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3539U, 601929031UL },
            new object[] { "Fallout4 - Textures9.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 68, 88, 49, 48 }, 3530U, 864276682UL },
            new object[] { "Fallout4 - Voices.ba2", new byte[] { 66, 84, 68, 88 }, 1U, new byte[] { 71, 78, 82, 76 }, 116101U, 2599842887UL },
        };

        #endregion
    }
}
