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
    /// <summary>
    /// Defines behaviour flags for BA2Loader methods.
    /// <see cref="BA2Loader"/>
    /// </summary>
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

    /// <summary>
    /// Contains methods to load BA2 archives.
    /// </summary>
    public static partial class BA2Loader
    {
        /// <summary>
        /// BA2 header signature (BTDX)
        /// </summary>
        internal static readonly byte[] HeaderSignature = { 0x42, 0x54, 0x44, 0x58 };

        /// <summary>
        /// Header size in bytes.
        /// </summary>
        internal static readonly int HeaderSize = 24;

        /// <summary>
        /// Excepted archive version.
        /// </summary>
        internal static readonly int ArchiveVersion = 1;

        /// <summary>
        /// Loads archive from file using custom settings.
        /// </summary>
        /// <see cref="BA2LoadException"/>
        /// <param name="filePath">Path to archive.</param>
        /// <param name="flags">Flags for loader.</param>
        /// <returns>BA2ArchiveBase instance.</returns>
        public static BA2Archive Load(string filePath, BA2LoaderFlags flags = BA2LoaderFlags.None)
        {
            try
            {
                var stream = File.OpenRead(filePath);
                return Load(stream, flags);
            }
            catch (IOException e)
            {
                throw new BA2LoadException("Cannot open file \"" + filePath + "\": " + e.Message, e);
            }
        }

        /// <summary>
        /// Load archive from stream.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when <c>stream</c> is null.</exception>
        private static BA2Archive Load(Stream stream, BA2LoaderFlags flags = BA2LoaderFlags.None)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            // file cannot be valid archive if header is less than HeaderSize
            if (stream.Length - stream.Position < HeaderSize)
                throw new BA2LoadException("Given stream cannot be valid archive.");

            BA2Archive archive = null;
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
            {
                BA2Header header = LoadHeader(reader);

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
                        } else {
                            throw new BA2LoadException("Archive of type \"" + Encoding.ASCII.GetString(header.ArchiveType) + "\" is not supported");
                        }
                        break;
                }

                archive.Header = header;
                archive.ArchiveStream = stream;

                archive.PreloadData(reader);
            }

            return archive;
        }

        /// <summary>
        /// Creates BA2Header from stream consumed by BinaryReader.
        /// </summary>
        /// <param name="reader">BinaryReader instance.</param>
        /// <returns>BA2Header instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <c>reader</c> is null.</exception>
        private static BA2Header LoadHeader(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            return new BA2Header()
            {
                Signature = reader.ReadBytes(4),
                Version = reader.ReadUInt32(),
                ArchiveType = reader.ReadBytes(4),
                TotalFiles = reader.ReadUInt32(),
                NameTableOffset = reader.ReadUInt64()
            };
        }
    }
}
