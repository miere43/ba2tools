using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ba2Tools;
using System.Diagnostics;
using System.Threading;

namespace BasicArchiveSample
{
    class ConsoleProgress : IProgress<int>
    {
        int value = 0;

        int totalFiles = 1;

        int prevCursorTop = 0;

        int prevCursorLeft = 0;

        double totalSizeInMB = 1;

        public ConsoleProgress(int totalFiles, uint totalSizeInBytes)
        {
            this.totalFiles = totalFiles;
            this.totalSizeInMB = (double)totalSizeInBytes / 1024 / 1024;
        }

        public void Start()
        {
            prevCursorTop = Console.CursorTop;
            prevCursorLeft = Console.CursorLeft;
        }

        public void Finish()
        {

        }

        public void Report(int value)
        {
            this.value = value;
            Console.SetCursorPosition(prevCursorLeft, prevCursorTop);
            Console.WriteLine("Hit any key to cancel.");
            double percent = (double)value / totalFiles;
            Console.WriteLine("{0:P2} | {1}/{2} | {3:0.00}/{4:0.00} MB",
                percent,
                value,
                totalFiles,
                totalSizeInMB * percent,
                totalSizeInMB);
        }
    }

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
            var listFileCount = Math.Min(5, filesInArchive.Count);

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
            if (Console.ReadLine().Trim().ToLower() == "y")
            {
                var cancel = new CancellationTokenSource();
                var progress = new ConsoleProgress((int)archive.TotalFiles, (uint)archive.ArchiveStream.Length);
                ManualResetEvent ev = new ManualResetEvent(false);
                bool cancelled = false;

                Thread thread = new Thread(() =>
                {
                    try {
                        progress.Start();
                        // extract
                        archive.ExtractAll("D:\\TestExtract", cancel.Token, progress, true);
                        // manual exit from thread after extracting.
                        ev.Set();
                    }
                    catch (OperationCanceledException e)
                    {
                        cancelled = true;
                        ev.Set();
                    }
                    finally
                    {
                        progress.Finish();
                        ev.Set();
                    }
                });

                thread.Start();

                var key = Console.ReadKey();
                if (!cancelled)
                {
                    Console.WriteLine("Cancelling...");
                    cancel.Cancel();
                }

                ev.WaitOne();

                if (cancelled)
                {
                    Console.WriteLine("Cancelled.");
                }
            }
        }
    }
}
