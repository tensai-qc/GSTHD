using GSTHD.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GSTHD
{
    public partial class MainForm : Form
    {
        readonly Dictionary<string, string> ListPlacesWithTag = new Dictionary<string, string>();
        readonly SortedSet<string> ListPlaces = new SortedSet<string>();
        readonly SortedSet<string> ListSometimesHintsSuggestions = new SortedSet<string>();

        MainForm_MenuBar MenuBar;
        LocalSettings LocalSettings;
        Layout ActiveLayout;
        Settings ActiveSettings;

        public MainForm()
        {
            InitializeComponent();

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowLocation = this.Location;
            }
            Properties.Settings.Default.Save();
        }

        private void Initialize(object sender, EventArgs e)
        {
            LoadSettings();
            LoadMenuBar();
            LoadActiveLayout();
        }

        private void LoadSettings()
        {
            LocalSettings = LoadLocalSettings();
            ActiveSettings = new Settings(LocalSettings);
        }

        private void LoadMenuBar()
        {
            MenuBar = new MainForm_MenuBar(this, LocalSettings)
            {
                Dock = DockStyle.Top
            };
            MenuBar.SetRenderer();
        }

        private void LoadActiveLayout()
        {
            Layout layout;
            try
            {
                layout = PreloadLayout(LocalSettings.ActiveLayout);
            }
            catch (GSTHDException ex) when (
                ex is FilesNotFoundException ||
                ex is InvalidLayoutFileException)
            {
                // do nothing, application will open in an empty no-layout state
                ShowErrorMessage(ex);
                return;
            }

            ActiveLayout = layout;
            PostloadLayout();
        }

        public void LoadLayout(string layoutPath)
        {
            Layout layout;
            try
            {
                layout = PreloadLayout(layoutPath);
            }
            catch (GSTHDException ex) when (
                ex is FilesNotFoundException ||
                ex is InvalidLayoutFileException)
            {
                // do nothing, stay on previous layout state
                ShowErrorMessage(ex);
                return;
            }

            Controls.Clear();
            ActiveLayout = layout;

            if (string.IsNullOrEmpty(layoutPath) || !Path.IsPathRooted(layoutPath))
            {
                LocalSettings.ActiveLayout = layoutPath;
            }
            else
            {
                string baseDir = Environment.CurrentDirectory;
                if (!string.Equals(Path.GetPathRoot(layoutPath), Path.GetPathRoot(baseDir),
                                StringComparison.OrdinalIgnoreCase))
                {
                    LocalSettings.ActiveLayout = layoutPath;
                }
                else
                {
                    var baseUri = new Uri(baseDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
                    var fileUri = new Uri(layoutPath);
                    LocalSettings.ActiveLayout = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString());
                }
            }

            JsonIO.Write(LocalSettings, LocalSettings.LocalSettingsFileName);
            PostloadLayout();
        }

        public void ReloadActiveLayout()
        {
            Layout layout;
            try
            {
                layout = PreloadLayout(LocalSettings.ActiveLayout);
            }
            catch (GSTHDException ex) when (
                ex is FilesNotFoundException ||
                ex is InvalidLayoutFileException)
            {
                // do nothing, stay on previous layout state
                ShowErrorMessage(ex);
                return;
            }

            Controls.Clear();
            ActiveLayout = layout;
            PostloadLayout();
        }

        private LocalSettings LoadLocalSettings()
        {
            ListPlaces.Clear();
            ListPlaces.Add("");
            ListPlacesWithTag.Clear();
            JObject json_places = JObject.Parse(File.ReadAllText(@"oot_places.json"));
            foreach (var property in json_places)
            {
                ListPlaces.Add(property.Key.ToString());
                ListPlacesWithTag.Add(property.Key, property.Value.ToString());
            }

            ListSometimesHintsSuggestions.Clear();
            JObject json_hints = JObject.Parse(File.ReadAllText(@"sometimes_hints.json"));
            foreach (var categorie in json_hints)
            {
                foreach (var hint in categorie.Value)
                {
                    ListSometimesHintsSuggestions.Add(hint.ToString());
                }
            }

            return JsonIO.Read<LocalSettings>(LocalSettings.LocalSettingsFileName);
        }

        private string FixLayoutPath(string layoutPath)
        {
            if (File.Exists(layoutPath))
            {
                return layoutPath;
            }

            var fixedPath = $@"Layouts\{layoutPath}.json";
            if (File.Exists(fixedPath))
            {
                return fixedPath;
            }
            else throw new FilesNotFoundException(layoutPath, fixedPath);
        }

        private Layout PreloadLayout(string layoutPath)
        {
            var path = FixLayoutPath(layoutPath);

            var layout = new Layout(path);
            layout.LoadContents(ActiveSettings, ListSometimesHintsSuggestions, ListPlacesWithTag);
            return layout;
        }

        private void SetSize()
        {
            Size = new Size(ActiveLayout.Size.Width, ActiveLayout.Size.Height + MenuBar.Size.Height);
        }

        private bool IsOnScreen(Point location, Size size)
        {
            Rectangle rect = new Rectangle(location, size);
            return Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(rect));
        }

        private void SetLocation()
        {
            Point savedLocation = Properties.Settings.Default.WindowLocation;
            if (savedLocation != Point.Empty && IsOnScreen(savedLocation, this.Size))
            {
                this.Location = savedLocation;
            }
        }

        private void SetBackColor()
        {
            if (ActiveLayout.Settings.BackgroundColor.HasValue)
                BackColor = ActiveLayout.Settings.BackgroundColor.Value;
            ActiveLayout.BackColor = BackColor;
        }

        private void SetBackImage()
        {
            if (ActiveLayout.Settings.BackgroundImage != null)
            {
                PerformLayout();
                var size = ActiveLayout.ClientSize;

                using (var src = Image.FromFile(@"Resources/" + ActiveLayout.Settings.BackgroundImage))
                {
                    var scaled = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
                    using (var g = Graphics.FromImage(scaled))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(src, new Rectangle(Point.Empty, size));
                    }

                    BackgroundImage = scaled;
                    ActiveLayout.BackgroundImageLayout = ImageLayout.None;
                    ActiveLayout.BackgroundImage = scaled;
                }
            }
        }

        private void PostloadLayout()
        {
            ActiveLayout.Dock = DockStyle.Top;
            SetSize();
            SetLocation();
            SetBackColor();

            ActiveSettings.SetLayoutSettings(ActiveLayout.Settings);

            Controls.Add(ActiveLayout);
            Controls.Add(MenuBar);
            SetBackImage();
        }

        public void UpdateSettings()
        {
            ActiveSettings.Update();
            ActiveLayout.UpdateFromSettings();
        }

        public void Reset()
        {
            ActiveLayout.BackgroundImage = null;
            ControlExtensions.ClearAndDispose(ActiveLayout);
            ReloadActiveLayout();
            Process.GetCurrentProcess().Refresh();
        }

        public void ShowErrorMessage(GSTHDException ex)
        {
            MessageBox.Show(ex.Message, $"{Config.ErrorMessageTitlePrefix}: {ex.Title}");
        }
    }

    public class FilesNotFoundException : GSTHDException
    {
        private static string GetMessage(string[] filePaths)
        {
            if (filePaths.Length == 0)
                return GenericMessage;

            var sb = new StringBuilder();
            foreach (var filePath in filePaths)
            {
                sb.Append($"File \"{filePath}\" not found. ");
            }

            return sb.ToString();
        }

        private static readonly string GenericMessage = $"File(s) not found.";

        public FilesNotFoundException(params string[] fileNames)
            : base(Config.LayoutFileExceptionTitle, GetMessage(fileNames)) { }
        public FilesNotFoundException(string[] fileNames, Exception inner)
            : base(Config.LayoutFileExceptionTitle, GetMessage(fileNames), inner) { }
    }
}
