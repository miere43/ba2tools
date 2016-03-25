using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ba2Tools.Internal;

namespace Ba2Tools
{
    public static partial class BA2Loader
    {
        /// <summary>
        /// Contains all known BA2 archive signatures and their representation in
        /// BA2Tools library.
        /// </summary>
        private static readonly Dictionary<byte[], BA2Type> ArchiveSignatures;

        /// <summary>
        /// Initializes the <see cref="BA2Loader"/> class.
        /// </summary>
        static BA2Loader()
        {
            ArchiveSignatures =
                new Dictionary<byte[], BA2Type>(new ByteSequenceEqualityComparer())
            {
                // BTDX
                { new byte[4] { 0x47, 0x4E, 0x52, 0x4C }, BA2Type.General },
                // DX10
                { new byte[4] { 0x44, 0x58, 0x31, 0x30 }, BA2Type.Texture }
            };
        }

        /// <summary>
        /// Resolve archive type from header signature.
        /// </summary>
        /// <param name="signature">Archive signature.</param>
        /// <returns>
        /// Archive type.
        /// </returns>
        /// <see cref="BA2Type" />
        public static BA2Type GetArchiveType(byte[] signature)
        {
            if (ArchiveSignatures.ContainsKey(signature))
                return ArchiveSignatures[signature];

            return BA2Type.Unknown;
        }

        /// <summary>
        /// Resolve archive type from Ba2ArchiveBase derived class instance.
        /// </summary>
        /// <param name="archive">The archive.</param>
        /// <returns>
        /// Archive type.
        /// </returns>
        /// <see cref="BA2Type" />
        public static BA2Type GetArchiveType(BA2Archive archive)
        {
            var archiveType = archive.GetType();
            if (archiveType == typeof(BA2GeneralArchive))
                return BA2Type.General;
            else if (archiveType == typeof(BA2TextureArchive))
                return BA2Type.Texture;

            return BA2Type.Unknown;
        }

        /// <summary>
        /// Resolve archive type from file.
        /// </summary>
        /// <param name="filePath">Path to the archive.</param>
        /// <returns>
        /// Archive type.
        /// </returns>
        public static BA2Type GetArchiveType(string filePath)
        {
            BA2Header header;
            FileStream stream = null;

            try {
                stream = File.OpenRead(filePath);
                if (stream.Length < BA2Loader.HeaderSize)
                    return BA2Type.Unknown;

                using (var reader = new BinaryReader(stream, Encoding.ASCII)) {
                    stream = null;
                    header = LoadHeader(reader);
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }

            return GetArchiveType(header.Signature);
        }

        /// <summary>
        /// Resolve archive type from generic type.
        /// </summary>
        /// <returns>
        /// Archive type.
        /// </returns>
        public static BA2Type GetArchiveType<T>() where T : BA2Archive
        {
            var type = typeof(T);
            if (typeof(BA2GeneralArchive) == type)
            {
                return BA2Type.General;
            }
            else if (typeof(BA2TextureArchive) == type)
            {
                return BA2Type.Texture;
            }

            return BA2Type.Unknown;
        }
    }
}

