using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ba2Tools.Writers
{
    [Serializable]
    public class BA2WriteException : Exception
    {
        public BA2WriteException() { }
        public BA2WriteException(string message) : base(message) { }
        public BA2WriteException(string message, Exception inner) : base(message, inner) { }
        protected BA2WriteException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
