using Ba2Tools.ArchiveTypes;
using Ba2Tools.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Tools
{
    [Flags]
    public enum Ba2ArchiveLoaderFlags
    {
        None = 0,
        /// <summary>
        /// Ignore archive version 
        /// </summary>
        IgnoreVersion = 1,
        /// <summary>
        /// Load unknown archive types. Ba2ArchiveBase instance will be returned 
        /// instead of throwing exception.
        /// </summary>
        LoadUnknownArchiveTypes = 2,
    }

    public static partial class Ba2ArchiveLoader
    {
        /// <summary>
        /// BA2 header signature (BTDX)
        /// </summary>
        internal static readonly byte[] HeaderSignature = { 0x42, 0x54, 0x44, 0x58 };

        internal static readonly uint HeaderSize = 24;

        internal static readonly uint ArchiveVersion = 1;

        private struct Ba2ArchiveHeader
        {
            public byte[] Signature;

            public UInt32 Version;

            public byte[] ArchiveType;

            public UInt32 TotalFiles;

            public UInt64 NameTableOffset;
        }

        //public static Ba2ArchiveBase Load(string filePath, out Ba2ArchiveType archiveType)
        //{

        //}

        private static Ba2ArchiveHeader LoadHeader(BinaryReader reader)
        {
            return new Ba2ArchiveHeader()
            {
                Signature = reader.ReadBytes(4),
                Version = reader.ReadUInt32(),
                ArchiveType = reader.ReadBytes(4),
                TotalFiles = reader.ReadUInt32(),
                NameTableOffset = reader.ReadUInt64()
            };
        }

        public static Ba2ArchiveBase Load(string filePath)
        {
            return Load(filePath, Ba2ArchiveLoaderFlags.None);
        }

        /// <summary>
        /// Loads BA2 archive.
        /// </summary>
        /// <see cref="Ba2ArchiveLoadException"/>
        /// <param name="filePath">Path to archive</param>
        /// <param name="flags">Flags for loader</param>
        /// <returns>BA2ArchiveBase instance</returns>
        public static Ba2ArchiveBase Load(string filePath, Ba2ArchiveLoaderFlags flags)
        {
            FileStream fileStream = null;
            try {
                fileStream = File.OpenRead(filePath);
            } catch (IOException e) {
                throw new Ba2ArchiveLoadException("Cannot open file \"" + filePath + "\": " + e.Message, e);
            }

            // file cannot be valid archive if header is less than HeaderSize
            if (fileStream.Length - fileStream.Position < HeaderSize)
                throw new Ba2ArchiveLoadException("\"" + filePath + "\" cannot be valid BA2 archive");

            Ba2ArchiveBase archive = null;
            using (BinaryReader reader = new BinaryReader(fileStream, Encoding.ASCII))
            {
                fileStream.Lock(0, HeaderSize);

                Ba2ArchiveHeader header = LoadHeader(reader);

                fileStream.Unlock(0, HeaderSize);

                if (!HeaderSignature.SequenceEqual(header.Signature))
                    throw new Ba2ArchiveLoadException("Archive has invalid signature");

                if (header.Version != ArchiveVersion && !flags.HasFlag(Ba2ArchiveLoaderFlags.IgnoreVersion))
                    throw new Ba2ArchiveLoadException("Version of archive is not valid (\"" + header.Version.ToString() + "\")");

                // compare excepted signature and file signature
                switch (GetArchiveType(header.ArchiveType))
                {
                    case Ba2ArchiveType.General:
                        archive = new Ba2GeneralArchive();
                        break;
                    case Ba2ArchiveType.Texture:
                        // TODO
                        archive = new Ba2TextureArchive();
                        break;
                    case Ba2ArchiveType.Unknown:
                    default:
                        if (flags.HasFlag(Ba2ArchiveLoaderFlags.LoadUnknownArchiveTypes))
                        {
                            archive = new Ba2ArchiveBase();
                        }
                        else
                        {
                            throw new Ba2ArchiveLoadException("Archive of type \"" + Encoding.ASCII.GetString(header.ArchiveType) + "\" is not supported");
                        }
                        break;
                }

                archive.TotalFiles = header.TotalFiles;
                archive.Version = header.Version;
                archive.NameTableOffset = header.NameTableOffset;
                archive.FilePath = filePath;

                archive.PreloadData(reader);
            }

            return archive;
        }
    }
}
