using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ba2Explorer.Commands
{
    public class HelpCommand : ICommand
    {
        MainForm form;

        public HelpCommand(MainForm form)
        {
            this.form = form;
        }

        public bool CanInvoke()
        {
            return true;
        }

        public void Invoke()
        {
            MessageBox.Show("NO HELP FOR YOU");
        }
    }
}
