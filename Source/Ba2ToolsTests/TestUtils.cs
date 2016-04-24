using Ba2Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2ToolsTests
{
    public static class TestUtils
    {
        public static void AssertExtractedTextFile(BA2Archive archive, int fileIndex, string excepted)
        {
            using (var stream = new MemoryStream())
            {
                Assert.IsTrue(archive.ExtractToStream(fileIndex, stream), $"Unable to extract file { archive.FileList[fileIndex] }.");
                AssertExtractedTextFile(stream, excepted);
            }
        }

        public static void AssertExtractedTextFile(Stream stream, string excepted)
        {
            byte[] buffer = new byte[stream.Length];
            Assert.AreEqual(stream.Length, stream.Read(buffer, 0, (int)stream.Length));

            string actual = Encoding.ASCII.GetString(buffer);

            Assert.IsTrue(actual.Equals(excepted, StringComparison.Ordinal), $"Excepted \"{ excepted }\", but got \"{ actual }\"");
        }

        public static void AssertFileListEntries(BA2Archive archive, IList<string> excepted)
        {
            var actual = archive.FileList;
            Assert.AreEqual<int>(excepted.Count(), actual.Count(), "Name table entries count are not same.");
            for (int i = 0; i < excepted.Count; i++)
            {
                string exceptFile = excepted[i];
                string actualFile = actual[i];

                bool ok = exceptFile.Equals(actualFile, StringComparison.OrdinalIgnoreCase);
                if (!ok)
                    Assert.Fail($"Excepted name table entry \"{ exceptFile }\", but got \"{ actualFile }\" (index { i }).");
            }
        }
    }
}
