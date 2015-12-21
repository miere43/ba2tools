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

        /// <summary>
        /// Header size in bytes.
        /// </summary>
        internal static readonly uint HeaderSize = 24;

        /// <summary>
        /// Excepted archive version.
        /// </summary>
        internal static readonly uint ArchiveVersion = 1;

        public struct Ba2ArchiveHeader
        {
            public byte[] Signature;

            public UInt32 Version;

            public byte[] ArchiveType;

            public UInt32 TotalFiles;

            public UInt64 NameTableOffset;
        }

        /// <summary>
        /// Fills Ba2ArchiveHeader struct using BinaryReader.
        /// </summary>
        /// <param name="reader">BinaryReader instance.</param>
        /// <returns></returns>
        private static Ba2ArchiveHeader LoadHeader(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return new Ba2ArchiveHeader()
            {
                Signature = reader.ReadBytes(4),
                Version = reader.ReadUInt32(),
                ArchiveType = reader.ReadBytes(4),
                TotalFiles = reader.ReadUInt32(),
                NameTableOffset = reader.ReadUInt64()
            };
        }

        /// <summary>
        /// Load archive from path using default settings.
        /// </summary>
        /// <param name="filePath">Path to archive.</param>
        /// <returns>BA2ArchiveBase instance.</returns>
        public static Ba2ArchiveBase Load(string filePath)
        {
            return Load(filePath, Ba2ArchiveLoaderFlags.None);
        }

        /// <summary>
        /// Loads archive from path using custom settings.
        /// </summary>
        /// <see cref="Ba2ArchiveLoadException"/>
        /// <param name="filePath">Path to archive.</param>
        /// <param name="flags">Flags for loader.</param>
        /// <returns>BA2ArchiveBase instance.</returns>
        public static Ba2ArchiveBase Load(string filePath, Ba2ArchiveLoaderFlags flags)
        {
            FileStream archiveStream = null;
            try {
                archiveStream = File.OpenRead(filePath);
            } catch (IOException e) {
                throw new Ba2ArchiveLoadException("Cannot open file \"" + filePath + "\": " + e.Message, e);
            }

            // file cannot be valid archive if header is less than HeaderSize
            if (archiveStream.Length - archiveStream.Position < HeaderSize)
                throw new Ba2ArchiveLoadException("\"" + filePath + "\" cannot be valid BA2 archive");

            Ba2ArchiveBase archive = null;
            using (BinaryReader reader = new BinaryReader(archiveStream, Encoding.ASCII))
            {
                archiveStream.Lock(0, HeaderSize);

                Ba2ArchiveHeader header = LoadHeader(reader);

                archiveStream.Unlock(0, HeaderSize);

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

                archive.Header = header;
                archive.FilePath = filePath;

                archive.PreloadData(reader);
            }

            return archive;
        }
    }
}
