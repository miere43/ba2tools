using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    /// <summary>
    /// BA2 archive loader exception for errors during archive loading.
    /// </summary>
    [Serializable]
    public class BA2LoadException : Exception
    {
        public BA2LoadException() { }
        public BA2LoadException(string message) : base(message) { }
        public BA2LoadException(string message, Exception inner) : base(message, inner) { }
        protected BA2LoadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
