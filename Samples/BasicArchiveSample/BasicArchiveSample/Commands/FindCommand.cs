using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicArchiveSample.Commands
{
    public class FindCommand : Command
    {
        public string Search { get; set; }

        public FindCommand(string search)
            : base(CommandType.Find)
        {
            Search = search;
        }
    }
}
