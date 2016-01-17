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

            Console.Write("Extract " + archive.TotalFiles + " files to \"D:\\TestExtract\"? (y/n)\n> ");
            if (Console.ReadLine().Trim() == "y")
                archive.ExtractAll("D:\\TestExtract", overwriteFiles: true);
        }
    }
}
