using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    [Serializable]
    public class Ba2ArchiveLoadException : Exception
    {
        public Ba2ArchiveLoadException() { }
        public Ba2ArchiveLoadException(string message) : base(message) { }
        public Ba2ArchiveLoadException(string message, Exception inner) : base(message, inner) { }
        protected Ba2ArchiveLoadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
