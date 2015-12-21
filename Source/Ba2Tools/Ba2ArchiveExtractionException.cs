using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// Exception for BA2 archive extraction errors.
    /// </summary>
    [Serializable]
    public class Ba2ArchiveExtractionException : Exception
    {
        public Ba2ArchiveExtractionException() { }
        public Ba2ArchiveExtractionException(string message) : base(message) { }
        public Ba2ArchiveExtractionException(string message, Exception inner) : base(message, inner) { }
        protected Ba2ArchiveExtractionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
