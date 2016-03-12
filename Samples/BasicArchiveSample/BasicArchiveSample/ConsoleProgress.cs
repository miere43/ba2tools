using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
