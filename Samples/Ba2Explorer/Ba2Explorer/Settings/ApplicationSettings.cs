using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ba2Explorer.Settings.Serializer;
using System.Drawing;

namespace Ba2Explorer.Settings
{
    public class ApplicationSettings
    {
        /// <summary>
        /// Gets or sets the width of the window. -1 means default value should be used.
        /// </summary>
        /// <value>
        /// The width of the window.
        /// </value>
        [SerializeSettingsProperty]
        public int WindowWidth { get; set; } = -1;

        /// <summary>
        /// Gets or sets the height of the window. -1 means default value should be used.
        /// </summary>
        /// <value>
        /// The height of the window.
        /// </value>
        [SerializeSettingsProperty]
        public int WindowHeight { get; set; } = -1;

        [SerializeSettingsProperty]
        public Point WindowLocation { get; set; } = new Point(int.MinValue, int.MinValue);

        private string openArchiveInitialPath = null;
        [SerializeSettingsProperty()]
        public string OpenArchiveInitialPath
        {
            get { return openArchiveInitialPath; }
            set
            {
                char[] invalidChars = System.IO.Path.GetInvalidPathChars();
                foreach (char c in value)
                    if (invalidChars.Contains(c))
                    {
                        System.Diagnostics.Debug.WriteLine("open archive initial path contains invalid chars");
                        openArchiveInitialPath = null;
                        return;
                    }

                openArchiveInitialPath = System.IO.Path.GetDirectoryName(value);
            }
        }
    }
}
