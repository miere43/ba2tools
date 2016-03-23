using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicArchiveSample.Commands
{
    public class ExtractAllCommand : Command
    {
        public string Destination { get; private set; }

        public ExtractAllCommand(string dest)
            : base(CommandType.ExportAll)
        {
            Destination = dest;
        }
    }
}
