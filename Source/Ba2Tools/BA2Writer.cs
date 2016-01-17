using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ba2Tools
{
    public class BA2Writer
    {
        public BA2Header Header { get; protected set; } 

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Doesn't change input string in any way.</remarks>
        /// <param name="path"></param>
        protected void IsValidPath(ref string path)
        {
        }

        protected void NormalizePath(ref string path)
        {
            path = path.Replace('/', '\\');
        }

        protected internal void SerializeHeader(BA2Header header, BinaryWriter writer)
        {
            
        }
    }
}
