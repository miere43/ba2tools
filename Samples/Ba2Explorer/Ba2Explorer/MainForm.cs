using Ba2Explorer.Commands;
using Ba2Explorer.Settings;
using Ba2Explorer.Settings.Serializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Ba2Explorer
{
    public partial class MainForm : Form
    {
        public ApplicationSettings Settings = null;

        HelpCommand helpCommand;

        OpenArchiveCommand openArchiveCommand;

        private void LoadSettings()
        {
            IList<SettingsDeserializationError> errors = null;

            try
            {
                if (File.Exists("settings.xml"))
                {
                    using (var stream = File.OpenRead("settings.xml"))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        Settings = SettingsXmlSerializer
                            .Deserialize<ApplicationSettings>(stream, out errors);
                    }

                    foreach (var error in errors)
                    {
                        MessageBox.Show(error.Message, error.PropertyName + " property deserialization error");
                    }
                }
            }
            finally
            {
                if (Settings == null)
                    Settings = new ApplicationSettings();
            }
        }

        private void SaveSettings()
        {
            if (Settings != null)
            {
                Settings.WindowWidth = this.Width;
                Settings.WindowHeight = this.Height;
                Settings.WindowLocation = this.DesktopLocation;

                try
                {
                    using (var file = File.Create("settings.xml"))
                    {
                        file.Seek(0, SeekOrigin.Begin);
                        SettingsXmlSerializer.Serialize(Settings, file);
                    }
                }
                catch (IOException) { }
            }
        }

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            BindCommands();

            this.Text = "BA2 Explorer " + Utility.VersionFormatter.Format(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            if (Settings.WindowWidth > 0)
                this.Width = Settings.WindowWidth;
            if (Settings.WindowHeight > 0)
                this.Height = Settings.WindowHeight;
        }

        private void BindCommands()
        {
            helpCommand = new HelpCommand(this);
            helpMenuItem.Click += (sender, args) => helpCommand.Invoke();

            openArchiveCommand = new OpenArchiveCommand(this);
            fileOpenMenuItem.Click += (sender, args) => openArchiveCommand.Invoke();

            fileQuitMenuItem.Click += (sender, args) => this.Close();
        }

        protected override void OnShown(EventArgs e)
        {
            if (Settings != null)
            {
                if (Settings.WindowLocation.X != int.MinValue &&
                    Settings.WindowLocation.Y != int.MinValue)
                    this.DesktopLocation = Settings.WindowLocation;
            }

            base.OnShown(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
    }
}
