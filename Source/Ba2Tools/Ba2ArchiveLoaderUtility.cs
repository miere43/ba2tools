using System;
using System.Collections.Generic;
using System.Linq;
using Ba2Tools.Internal;
using Ba2Tools.ArchiveTypes;
using System.IO;
using System.Text;

namespace Ba2Tools
{
    public static partial class Ba2ArchiveLoader
    {
        private static readonly Dictionary<byte[], Ba2ArchiveType> ArchiveSignatures;

        static Ba2ArchiveLoader()
        {
            ArchiveSignatures =
                new Dictionary<byte[], Ba2ArchiveType>(new ByteSequenceEqualityComparer())
            {
                // BTDX
                { new byte[4] { 0x47, 0x4E, 0x52, 0x4C }, Ba2ArchiveType.General },
                // DX10
                { new byte[4] { 0x44, 0x58, 0x31, 0x30 }, Ba2ArchiveType.Texture }
            };
        }

        /// <summary>
        /// Resolve archive type from header signature.
        /// </summary>
        /// <param name="signature">Archive signature</param>
        /// <returns>Archive type or unknown</returns>
        public static Ba2ArchiveType GetArchiveType(byte[] signature)
        {
            if (ArchiveSignatures.ContainsKey(signature))
                return ArchiveSignatures[signature];

            return Ba2ArchiveType.Unknown;
        }

        /// <summary>
        /// Resolve archive type from Ba2ArchiveBase derived class instance.
        /// </summary>
        /// <param name="signature">Archive signature</param>
        /// <returns>Archive type or unknown</returns>
        public static Ba2ArchiveType GetArchiveType(Ba2ArchiveBase archive)
        {
            var archiveType = archive.GetType();
            if (archiveType == typeof(Ba2GeneralArchive))
                return Ba2ArchiveType.General;
            else if (archiveType == typeof(Ba2TextureArchive))
                return Ba2ArchiveType.Texture;

            return Ba2ArchiveType.Unknown;
        }

        /// <summary>
        /// Resolve archive type from file.
        /// </summary>
        /// <param name="signature">Archive signature</param>
        /// <returns>Archive type or unknown</returns>
        public static Ba2ArchiveType GetArchiveType(string filePath)
        {
            Ba2ArchiveHeader header;

            using (var stream = File.OpenRead(filePath)) {
                if (stream.Length < Ba2ArchiveLoader.HeaderSize)
                    return Ba2ArchiveType.Unknown;

                using (var reader = new BinaryReader(stream, Encoding.ASCII)) {
                    header = LoadHeader(reader);
                }
            }

            return GetArchiveType(header.Signature);
        }
    }
}

