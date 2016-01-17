using System;
using System.Collections.Generic;
using System.Linq;
using Ba2Tools.Internal;
using Ba2Tools;
using System.IO;
using System.Text;

namespace Ba2Tools
{
    public static partial class BA2Loader
    {
        private static readonly Dictionary<byte[], BA2Type> ArchiveSignatures;

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
        /// <param name="signature">Archive signature</param>
        /// <returns>Archive type or unknown</returns>
        public static BA2Type GetArchiveType(byte[] signature)
        {
            if (ArchiveSignatures.ContainsKey(signature))
                return ArchiveSignatures[signature];

            return BA2Type.Unknown;
        }

        /// <summary>
        /// Resolve archive type from Ba2ArchiveBase derived class instance.
        /// </summary>
        /// <param name="signature">Archive signature</param>
        /// <returns>Archive type or unknown</returns>
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
        /// <param name="signature">Archive signature</param>
        /// <returns>Archive type or unknown</returns>
        public static BA2Type GetArchiveType(string filePath)
        {
            BA2Header header;

            using (var stream = File.OpenRead(filePath)) {
                if (stream.Length < BA2Loader.HeaderSize)
                    return BA2Type.Unknown;

                using (var reader = new BinaryReader(stream, Encoding.ASCII)) {
                    header = LoadHeader(reader);
                }
            }

            return GetArchiveType(header.Signature);
        }
    }
}

