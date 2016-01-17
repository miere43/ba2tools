using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ba2Tools;
using System.IO;
using Ba2Tools;

namespace Ba2Tools.Writers
{
    public class BA2GeneralWriter : BA2Writer
    {

        public BA2GeneralWriter()
        {
            Header = new BA2Header();
        }

        //public static Ba2GeneralArchiveWriter FromExistingArchive(Ba2GeneralArchive archive)
        //{

        //}

        public void AddFromArchive(string path, BA2GeneralArchive archive)
        {

        }

        public void AddFile(string key, byte[] data)
        {
        }

        public void AddFile(string key, Stream data)
        {

        }

        public void Write(string path, bool overwrite = false)
        {
            if (overwrite == false && File.Exists(path))
                throw new BA2WriteException("Overwrite is not permitted.");


        }

        public void WriteToStream(Stream stream)
        {

        }
    }
}
