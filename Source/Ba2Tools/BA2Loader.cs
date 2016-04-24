﻿using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Ba2Tools
{
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
        /// Loads archive from file.
        /// </summary>
        /// <param name="filePath">Path to archive.</param>
        /// <param name="flags">Flags for loader.</param>
        /// <returns>BA2Archive instance.</returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="BA2LoadException"></exception>
        public static BA2Archive Load(string filePath, BA2LoaderFlags flags = BA2LoaderFlags.None)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            try
            {
                var stream = File.OpenRead(filePath);
                return Load(stream, flags);
            }
            catch (IOException e)
            {
                throw new BA2LoadException($"Cannot open file \"{filePath}\": {e.Message}", e);
            }
        }

        /// <summary>
        /// Loads archive of specified type from file.
        /// </summary>
        /// <see cref="BA2LoadException"/>
        /// <param name="filePath">Path to archive.</param>
        /// <param name="flags">Flags for loader.</param>
        /// <returns>BA2Archive instance.</returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="BA2LoadException" />
        public static T Load<T>(string filePath, BA2LoaderFlags flags = BA2LoaderFlags.None) where T : BA2Archive
        {
            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(nameof(filePath));

            try
            {
                var stream = File.OpenRead(filePath);
                return Load<T>(stream, flags);
            }
            catch (IOException e)
            {
                throw new BA2LoadException($"Cannot open file \"{filePath}\": {e.Message}", e);
            }
        }

        /// <summary>
        /// Load archive of specified type from stream.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="flags">Flags for loader.</param>
        /// <returns>BA2Archive instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <c>stream</c> is null.</exception>
        /// <exception cref="BA2LoadException" />
        public static T Load<T>(Stream stream, BA2LoaderFlags flags = BA2LoaderFlags.None) where T : BA2Archive
        {
            BA2Archive archive = Load(stream, flags);
            Type exceptedType = typeof(T);

            var exceptedArchive = archive as T;

            if (exceptedArchive != null)
                return exceptedArchive;
            else
            {
                if (archive != null)
                    archive.Dispose();

                throw new BA2LoadException(
                    string.Format("Loaded archive has type {0}, which is not same as requested type {1}",
                        GetArchiveType(archive),
                        GetArchiveType<T>()));
            }
        }

        /// <summary>
        /// Load archive from stream.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="flags"></param>
        /// <returns>BA2Archive instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <c>stream</c> is null.</exception>
        /// <exception cref="BA2LoadException" />
        public static BA2Archive Load(Stream stream, BA2LoaderFlags flags = BA2LoaderFlags.None)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            BA2Archive archive = null;
            try
            {
                // file cannot be valid archive if header is less than HeaderSize
                if (stream.Length - stream.Position < HeaderSize)
                    throw new BA2LoadException("Given stream cannot be valid archive.");

                using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
                {
                    BA2Header header = LoadHeader(reader);

                    if (!HeaderSignature.SequenceEqual(header.Signature))
                        throw new BA2LoadException("Archive has invalid signature");

                    if (header.Version != ArchiveVersion && !flags.HasFlag(BA2LoaderFlags.IgnoreVersion))
                        throw new BA2LoadException($"Version of archive is not valid (\"{header.Version.ToString()}\")");

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
                            if (flags.HasFlag(BA2LoaderFlags.IgnoreArchiveType))
                            {
                                archive = new BA2Archive();
                            }
                            else {
                                throw new BA2LoadException($"Archive of type \"{Encoding.ASCII.GetString(header.ArchiveType)}\" is not supported");
                            }
                            break;
                    }

                    if (flags.HasFlag(BA2LoaderFlags.Multithreaded))
                        archive.MultithreadedExtract = true;

                    archive.Header = header;
                    archive.ArchiveStream = stream;

                    archive.PreloadData(reader);
                }
            }
            catch (Exception)
            {
                if (archive != null)
                    archive.Dispose();
                throw;
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
