using System;
using System.Collections.Generic;
using System.Linq;

namespace Ba2Tools.Internal
{
    /// <summary>
    /// Byte array equality comparer.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.EqualityComparer{System.Byte[]}" />
    internal class ByteSequenceEqualityComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] x, byte[] y)
        {
            if (x == null || y == null)
                return x == y;
            return x.SequenceEqual(y);
        }

        public override int GetHashCode(byte[] obj)
        {
            if (obj.Length != 4)
                throw new NotImplementedException();

            return BitConverter.ToInt32(obj, 0);
        }
    }
}