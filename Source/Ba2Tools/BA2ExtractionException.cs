using System;

namespace Ba2Tools
{
    /// <summary>
    /// Exception for BA2 archive extraction errors.
    /// </summary>
    [Serializable]
    public class BA2ExtractionException : Exception
    {
        public BA2ExtractionException() { }
        public BA2ExtractionException(string message) : base(message) { }
        public BA2ExtractionException(string message, Exception inner) : base(message, inner) { }
        protected BA2ExtractionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
