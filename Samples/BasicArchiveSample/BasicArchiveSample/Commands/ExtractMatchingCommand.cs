using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicArchiveSample.Commands
{
    public class ExtractMatchingCommand : Command
    {
        public string FindString { get; private set; }

        public string Destination { get; private set; }

        public ExtractMatchingCommand(string destination, string findString)
            : base(CommandType.ExportMatching)
        {
            Destination = destination;
            FindString = findString;
        }
    }
}
