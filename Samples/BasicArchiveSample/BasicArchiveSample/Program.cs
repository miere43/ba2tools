using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ba2Tools;
using Ba2Tools.ArchiveTypes;
using System.Diagnostics;

namespace BasicArchiveSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //Stopwatch t = new Stopwatch();
            //t.Start();
            var temp = Ba2ArchiveLoader.Load(@"D:\Games\Fallout 4\Data\Fallout4 - Startup.ba2", Ba2ArchiveLoaderFlags.None);
            var s = temp.ListFiles();
            temp.ExtractAll("D:\\Vlad\\TestExtract", true);

            // temp.Extract(s, "D:\\");
            //Debug.WriteLine(s);
            //Debug.WriteLine(temp.ContainsFile("Interface\\BarterMenu.swf"));
            //temp.ExtractFile(@"interface\bartermenu.swf", @"D:\test", true);
            //t.Stop();
           // Debug.WriteLine($"Time elapsed: {t.Elapsed.Seconds}.{t.Elapsed.Milliseconds}");
//            if (args.Length < 1)
//            {
//                Console.WriteLine("Usage: <archive path>");
//                return;
//            }

            //            string archivePath = args[0];

            //            if (!File.Exists(archivePath))
            //            {
            //                Console.WriteLine("File \"{0}\" does not exists", archivePath);
            //                return;
            //            }

            //            Ba2ArchiveBase archive;
            //            try
            //            { 
            //                archive = Ba2ArchiveLoader.Load(archivePath, 
            //                    Ba2ArchiveLoaderFlags.LoadUnknownArchiveTypes |
            //                    Ba2ArchiveLoaderFlags.IgnoreVersion);
            //            }
            //            catch (Ba2ArchiveLoadException e)
            //            {
            //                Console.WriteLine("Cannot load archive: " + e.Message);
            //                return;
            //            }

            //            // Console.WriteLine("Signature: " + Encoding.ASCII.GetString(archive.Signature));
            //            Console.WriteLine("Version: " + archive.Version);

            //            Console.Write("Archive type: ");
            //            if (archive.GetType() == typeof(Ba2GeneralArchive))
            //                Console.WriteLine("General");
            //            else if (archive.GetType() == typeof(Ba2TextureArchive))
            //                Console.WriteLine("Texture");
            //            else
            //                Console.WriteLine("Unknown");

            //            Console.WriteLine("Total files: " + archive.FileCount);

            //            Console.ReadKey();

            //            Console.WriteLine("File names: ");
            //            foreach (var name in archive.ListFiles())
            //            {
            //                Console.WriteLine(name);
            //            }

            //#if DEBUG
            //            Console.WriteLine("Done. Press any key to continue...");
            //            Console.ReadKey();
            //#endif
        }
    }
}
