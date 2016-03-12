using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using BasicArchiveSample.Commands;
using Ba2Tools;

namespace BasicArchiveSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] passArgs = args;
            while (Inner(passArgs))
            {
                if (passArgs.Length != 0)
                    passArgs = new string[0];
                Inner(passArgs);
            }
        }

        static Command GetCommand()
        {
            Console.Write("> ");
            var input = Console.ReadLine().Trim();
            switch (input)
            {
                case "q":
                case "exit":
                    return new Command(CommandType.Exit);
                case "info":
                    return new Command(CommandType.Info);
                case "help":
                    return new Command(CommandType.Help);
                case "drop":
                    return new Command(CommandType.Drop);
            }

            #region find
            if (input.StartsWith("find"))
            {
                string[] args = GetArgs(input);
                if (args.Length == 1)
                {
                    WriteError("No args for command find passed.");
                    return Command.None;
                }
                else
                {
                    return new FindCommand(args[1]);
                }
            }
            #endregion

            #region extractall
            if (input.StartsWith("extractall"))
            {
                string[] args = GetArgs(input);
                if (args.Length == 1)
                {
                    WriteError("No destination path set.");
                    return Command.None;
                }
                else
                {
                    return new ExtractAllCommand(args[1]);
                }
            }
            #endregion

            #region extractmatching
            if (input.StartsWith("extractmatching"))
            {
                string[] args = GetArgs(input);
                if (args.Length == 1)
                {
                    WriteError("No args for extractmatching passed.");
                    return Command.None;
                }
                else if (args.Length == 2)
                {
                    WriteError("No destination path set.");
                    return Command.None;
                }
                else
                {
                    return new ExtractMatchingCommand(args[2], args[1]);
                }
            }
            #endregion

            #region extract
            if (input.StartsWith("extract"))
            {
                string[] args = GetArgs(input);
                if (args.Length == 1)
                {
                    WriteError("No args for command extract passed.");
                    return Command.None;
                }
                else if (args.Length == 2)
                {
                    WriteError("No destination folder set.");
                    return Command.None;
                }
                else
                {
                    return new ExtractCommand(args[1], args[2]);
                }
            }
            #endregion

            return Command.Invalid;
        }

        static BA2Archive RequestArchive()
        {
            string archivePath = null;
            BA2Archive archive = null;

            Console.WriteLine("Path to archive: ");
            while (true)
            {
                archivePath = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(archivePath))
                    continue;

                try
                {
                    archive = BA2Loader.Load(archivePath);
                }
                catch (Exception e)
                {
                    WriteError("Error while opening archive: {0}", e.Message);
                    goto tryAgain;
                }

                if (archive != null)
                    break;

                tryAgain:
                    Console.WriteLine("Try again: ");
            }

            return archive;
        }

        static bool Inner(string[] args)
        {
            BA2Archive archive = null;
            if (args.Length > 0)
            {
                try {
                    archive = BA2Loader.Load(args[0]);
                }
                catch (Exception e)
                {
                    WriteError("Invalid archive passed: {0}", e.Message);
                }
            }

            if (archive == null)
                archive = RequestArchive();

            using (archive)
            {
                bool listening = true;
                Console.WriteLine("Input command, \"q\" to exit, \"help\" to get help.");
                while (listening)
                {
                    Command command = GetCommand();
                    switch (command.Type)
                    {
                        case CommandType.Exit:
                            listening = false;
                            break;
                        case CommandType.Info:
                            PrintArchiveInfo(archive);
                            break;
                        case CommandType.Find:
                            FindFiles(archive, command as FindCommand);
                            break;
                        case CommandType.None:
                            break;
                        case CommandType.Export:
                            try
                            {
                                ExtractSingle(archive, command as ExtractCommand);
                                Console.WriteLine("Done.");
                            }
                            catch (Exception e)
                            {
                                WriteError("Error occur while exporting: {0}", e.Message);
                            }
                            break;
                        case CommandType.ExportMatching:
                            try
                            {
                                if (ExtractMatching(archive, command as ExtractMatchingCommand))
                                    Console.WriteLine("Done.");
                                else
                                    Console.WriteLine("No matching files found.");
                            }
                            catch (Exception e)
                            {
                                WriteError("Error occur while exporting: {0}", e.Message);
                            }
                            break;
                        case CommandType.ExportAll:
                            try
                            {
                                ExtractAll(archive, command as ExtractAllCommand);
                                Console.WriteLine("Done.");
                            }
                            catch (Exception e)
                            {
                                WriteError("Error occur while exporting: {0}", e.Message);
                            }
                            break;
                        case CommandType.Invalid:
                            WriteError("Unknown command");
                            break;
                        case CommandType.Drop:
                            return true;
                        case CommandType.Help:
                        default:
                            PrintHelp();
                            break;
                    }
                }
            }

            return false;
        }

        private static bool ExtractMatching(BA2Archive archive, ExtractMatchingCommand cmd)
        {
            var files = FindMatchingFiles(archive, cmd.FindString);
            if (files.Count() == 0)
            {
                return false;
            }

            int counter = 1;
            foreach (var fileName in files)
            {
                Console.WriteLine($"{counter++}. {fileName}");
            }

            ExtractFiles(archive, files, cmd.Destination);
            return true;
        }

        private static void ExtractAll(BA2Archive archive, ExtractAllCommand cmd)
        {
            ExtractFiles(archive, archive.ListFiles(), cmd.Destination);
        }

        private static void ExtractSingle(BA2Archive archive, ExtractCommand cmd)
        {
            archive.Extract(cmd.Source, cmd.Destination, true);
        }

        private static IEnumerable<string> FindMatchingFiles(BA2Archive archive, string match)
        {
            return archive.ListFiles().Where((fileName)
                => fileName.IndexOf(match, StringComparison.InvariantCultureIgnoreCase) != -1);
        } 

        private static bool ExtractFiles(BA2Archive archive, IEnumerable<string> files, string dest)
        {
            Console.Write($"Extract {files.Count()} files to \"{dest}\" (y/n)\n> ");
            if (Console.ReadLine().Trim().ToLower() == "y")
            {
                var cancel = new CancellationTokenSource();
                var progress = new ConsoleProgress((int)files.Count(), (uint)archive.ArchiveStream.Length);
                ManualResetEvent ev = new ManualResetEvent(false);
                bool cancelled = false;

                Thread thread = new Thread(() =>
                {
                    try
                    {
                        progress.Start();
                        // extract
                        archive.ExtractFiles(files, dest, cancel.Token, progress, true);
                        // manual exit from thread after extracting.
                        ev.Set();
                    }
                    catch (OperationCanceledException)
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

                var inputThread = new Thread(() =>
                {
                    var key = Console.ReadKey();
                    if (!cancelled)
                    {
                        Console.SetCursorPosition(Math.Max(0, Console.CursorLeft - 1), Console.CursorTop);

                        Console.WriteLine("Cancelling...");
                        cancel.Cancel();
                    }
                });
                inputThread.Start();

                ev.WaitOne();
                if (inputThread.IsAlive)
                    inputThread.Abort();

                if (cancelled)
                {
                    Console.WriteLine("Cancelled.");
                }

                return true;
            }

            return false;
        }

        private static void FindFiles(BA2Archive archive, FindCommand cmd)
        {
            int counter = 1;
            foreach (string file in FindMatchingFiles(archive, cmd.Search))
            {
                Console.WriteLine($"{counter++}. {file}");
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine($"Commands: info, drop, help, exit, q, find <str>, extract <file> <dest>, extractall <dest>, extractmatching <str> <dest>, clear");
        }

        static void PrintArchiveInfo(BA2Archive archive)
        {
            Console.WriteLine("Type: " + BA2Loader.GetArchiveType(archive));
            Console.WriteLine("Signature: " + Encoding.ASCII.GetString(archive.Header.Signature));
            Console.WriteLine("Total files: " + archive.TotalFiles);
            Console.WriteLine("Size: " + archive.ArchiveStream.Length / 1024 / 1024 + " MB");
        }

        static void WriteError(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(format, args);
            Console.ResetColor();
        }

        static string[] GetArgs(string str)
        {
            return str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
