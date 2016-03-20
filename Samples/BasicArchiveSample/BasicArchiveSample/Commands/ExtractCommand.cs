using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicArchiveSample.Commands
{
    public class ExtractCommand : Command
    {
        public string Source { get; private set; }

        public string Destination { get; private set; }

        public ExtractCommand(string src, string dest)
            : base(CommandType.Export)
        {
            Source = src;
            Destination = dest;
        }
    }
}
