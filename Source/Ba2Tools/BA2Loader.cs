using Ba2Tools;
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
    public enum BA2LoaderFlags
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

    public static partial class BA2Loader
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

        /// <summary>
        /// Fills Ba2ArchiveHeader struct using BinaryReader.
        /// </summary>
        /// <param name="reader">BinaryReader instance.</param>
        /// <returns></returns>
        private static BA2Header LoadHeader(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return new BA2Header()
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
        public static BA2Archive Load(string filePath)
        {
            return Load(filePath, BA2LoaderFlags.None);
        }

        /// <summary>
        /// Loads archive from path using custom settings.
        /// </summary>
        /// <see cref="BA2LoadException"/>
        /// <param name="filePath">Path to archive.</param>
        /// <param name="flags">Flags for loader.</param>
        /// <returns>BA2ArchiveBase instance.</returns>
        public static BA2Archive Load(string filePath, BA2LoaderFlags flags)
        {
            FileStream archiveStream = null;
            try {
                archiveStream = File.OpenRead(filePath);
            } catch (IOException e) {
                throw new BA2LoadException("Cannot open file \"" + filePath + "\": " + e.Message, e);
            }

            // file cannot be valid archive if header is less than HeaderSize
            if (archiveStream.Length - archiveStream.Position < HeaderSize)
                throw new BA2LoadException("\"" + filePath + "\" cannot be valid BA2 archive");

            BA2Archive archive = null;
            using (BinaryReader reader = new BinaryReader(archiveStream, Encoding.ASCII))
            {
                archiveStream.Lock(0, HeaderSize);

                BA2Header header = LoadHeader(reader);

                archiveStream.Unlock(0, HeaderSize);

                if (!HeaderSignature.SequenceEqual(header.Signature))
                    throw new BA2LoadException("Archive has invalid signature");

                if (header.Version != ArchiveVersion && !flags.HasFlag(BA2LoaderFlags.IgnoreVersion))
                    throw new BA2LoadException("Version of archive is not valid (\"" + header.Version.ToString() + "\")");

                // compare excepted signature and file signature
                switch (GetArchiveType(header.ArchiveType))
                {
                    case BA2Type.General:
                        archive = new BA2GeneralArchive();
                        break;
                    case BA2Type.Texture:
                        // TODO
                        archive = new BA2TextureArchive();
                        break;
                    case BA2Type.Unknown:
                    default:
                        if (flags.HasFlag(BA2LoaderFlags.LoadUnknownArchiveTypes))
                        {
                            archive = new BA2Archive();
                        }
                        else
                        {
                            throw new BA2LoadException("Archive of type \"" + Encoding.ASCII.GetString(header.ArchiveType) + "\" is not supported");
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
