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
        public static void AssertExtractedTextFile(BA2Archive archive, string fileName, string excepted)
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
    }
}
