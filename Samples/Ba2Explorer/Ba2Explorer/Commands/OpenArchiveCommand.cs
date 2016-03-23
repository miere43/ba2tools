using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ba2Explorer.Commands
{
    public class OpenArchiveCommand
    {
        MainForm form;

        public OpenArchiveCommand(MainForm form)
        {
            this.form = form;
        }

        public void Invoke()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "BA2 archives|*.ba2|All files|*.*";
            dialog.Title = "Select archive";
            if (!string.IsNullOrWhiteSpace(form.Settings.OpenArchiveInitialPath))
            {
                dialog.InitialDirectory = form.Settings.OpenArchiveInitialPath;
            }

            var result = dialog.ShowDialog(form);
            if (result == DialogResult.OK)
            {
                string path = dialog.FileName;
                form.Settings.OpenArchiveInitialPath =
                    Path.GetDirectoryName(path);
            }
        }
    }
}
