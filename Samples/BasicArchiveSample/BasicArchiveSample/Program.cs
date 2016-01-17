using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ba2Tools;
using System.Diagnostics;

namespace BasicArchiveSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Inner(args);

            Console.WriteLine("Done.");
#if DEBUG
            Console.ReadLine();
#endif
        }

        static void Inner(string[] args)
        {
            string archivePath = args.Length == 0 ? @"D:\Games\Fallout 4\Data\Fallout4 - Textures1.ba2" : args[0];

            // Load archive
            BA2Archive archive;
            try
            {
                archive = BA2Loader.Load(archivePath, BA2LoaderFlags.None);
            }
            catch (BA2LoadException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Cannot read archive " + archivePath + ": " + e.Message);
                Console.ResetColor();
                return;
            }
            // Base archive was loaded: you don't know what type of archive is (general or texture);

            // You anyway can list files in archive because all archives contains filenames;
            var filesInArchive = archive.ListFiles();
            var listFileCount = Math.Min(5, filesInArchive.Length);

            Console.WriteLine("First " + listFileCount + " files in archive: ");
            for (int i = 0; i < listFileCount; i++)
            {
                Console.WriteLine(i + 1 + ". " + filesInArchive[i]);
            }

            // Find out what archive type is it:
            if (archive as BA2GeneralArchive != null)
            {
                Console.WriteLine("archive type is general");
            }
            else if (archive.GetType() == typeof(BA2TextureArchive))
            {
                Console.WriteLine("archive type is texture");
            }
            else
            {
                var archiveType = BA2Loader.GetArchiveType(archive);
                Console.WriteLine("not supported archive type: " + archiveType);
                return;
            }

            Stopwatch s = new Stopwatch();
            s.Start();
            archive.ExtractAll(@"D:\Hello", true);
            s.Stop();
            Debug.WriteLine("elapsed: " + s.ElapsedMilliseconds);

            //Console.Write("Extract " + archive.TotalFiles + " files to \"D:\\Extract\"? (y/n)\n> ");
            //if (Console.ReadLine().Trim() == "y")
            //    archive.ExtractAll("D:\\TestExtract", overwriteFiles: true);


            //temp.Extract(s, "D:\\");
            //Debug.WriteLine(s);
            //Debug.WriteLine(temp.ContainsFile("Interface\\BarterMenu.swf"));
            //temp.ExtractFile(@"interface\bartermenu.swf", @"D:\test", true);
            //t.Stop();
            //Debug.WriteLine($"Time elapsed: {t.Elapsed.Seconds}.{t.Elapsed.Milliseconds}");
            //if (args.Length < 1)
            //{
            //    Console.WriteLine("Usage: <archive path>");
            //    return;
            //}

            //string archivePath = args[0];

            //if (!File.Exists(archivePath))
            //{
            //    Console.WriteLine("File \"{0}\" does not exists", archivePath);
            //    return;
            //}

            //Ba2ArchiveBase archive;
            //try
            //{
            //    archive = Ba2ArchiveLoader.Load(archivePath,
            //        Ba2ArchiveLoaderFlags.LoadUnknownArchiveTypes |
            //        Ba2ArchiveLoaderFlags.IgnoreVersion);
            //}
            //catch (Ba2ArchiveLoadException e)
            //{
            //    Console.WriteLine("Cannot load archive: " + e.Message);
            //    return;
            //}

            //// Console.WriteLine("Signature: " + Encoding.ASCII.GetString(archive.Signature));
            //Console.WriteLine("Version: " + archive.Version);

            //Console.Write("Archive type: ");
            //if (archive.GetType() == typeof(Ba2GeneralArchive))
            //    Console.WriteLine("General");
            //else if (archive.GetType() == typeof(Ba2TextureArchive))
            //    Console.WriteLine("Texture");
            //else
            //    Console.WriteLine("Unknown");

            //Console.WriteLine("Total files: " + archive.FileCount);

            //Console.ReadKey();

            //Console.WriteLine("File names: ");
            //foreach (var name in archive.ListFiles())
            //{
            //    Console.WriteLine(name);
            //}
        }
    }
}
