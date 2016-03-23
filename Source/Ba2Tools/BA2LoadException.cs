using System;

namespace Ba2Tools
{
    /// <summary>
    /// BA2 archive loader exception for errors during archive loading.
    /// <see cref="BA2Loader" />
    /// </summary>
    /// <seealso cref="System.Exception" />
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
