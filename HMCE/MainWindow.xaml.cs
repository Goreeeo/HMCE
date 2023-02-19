using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HMCE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string paradoxFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive");
        private readonly string editorDocumentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Goreo", "HMCE");
        private readonly string projectCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Goreo", "HMCE", "RecentProjects.txt");

        public static string activeProject = "";

        public delegate void PathDelegate(string path);

        private enum FileType
        {
            File, Folder
        }

        private class RecentDescriptorButton : MenuItem
        {
            private readonly string path;
            private readonly PathDelegate callback;

            public RecentDescriptorButton(string[] projectAttributes, PathDelegate callback)
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 8, 206));
                BorderThickness = new Thickness(0);
                Header = projectAttributes[0] + " - " + projectAttributes[1];
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                path = projectAttributes[0];
                this.callback = callback;
                Click += OnClick;
            }

            private void OnClick(object sender, RoutedEventArgs e)
            {
                callback(path);
            }
        }

        private class FileExplorerButton : TreeViewItem
        {
            private readonly string path;

            public FileExplorerButton(string path, string fileName, FileType type)
            {
                Background = new SolidColorBrush(Color.FromRgb(6, 0, 25));
                BorderThickness = new Thickness(0);
                Header = fileName;
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                if (type == FileType.File)
                {
                    MouseDoubleClick += OnFileClick;
                }
                this.path = path;
            }

            public void Add(FileExplorerButton button)
            {
                Items.Add(button);
            }

            private void OnFileClick(object sender, MouseButtonEventArgs e)
            {
                ScintillaNET.Scintilla scintilla = ScintillaManager.GetScintilla();
                string content = File.ReadAllText(path);
                scintilla.Text = content;
                ScintillaManager.SetDocument(path);
            }
        }

        public enum ProjectType
        {
            Undefined,
            HeartsOfIronIV,
            CrusaderKingsIII,
            EuropaUniversalisIV,
            VictoriaIII,
        }

        public static ProjectType projectType;

        public MainWindow()
        {
            InitializeComponent();
            GetRecentProjects();
            ScintillaManager.Init(scintilla);
            HMCE.FocusTreeViewer.Init(FocusTreeViewer);
            new Focus("ass", "GFX_focus_icon_name", null, null, null, "0", "0");
            FocusCollection.OnCompleteRegister();
        }

        private void GetRecentProjects()
        {
            RecentDescriptors.Items.Clear();

            if (!Directory.Exists(editorDocumentDirectory))
            {
                Directory.CreateDirectory(editorDocumentDirectory);
            }

            if (!File.Exists(projectCache))
            {
                File.Create(projectCache);
            }

            string cache = File.ReadAllText(projectCache);
            if (!string.IsNullOrEmpty(cache))
            {
                string[] cachedProjects = cache.Split('?');

                foreach (string project in cachedProjects)
                {
                    RecentDescriptors.Items.Add(new RecentDescriptorButton(project.Split('|'), LoadModFile));
                }
            }
        }

        public static ProjectType GetProjectTypeFromName(string name)
        {
            switch (name.ToLower())
            {
                case "hearts of iron iv":
                    {
                        return ProjectType.HeartsOfIronIV;
                    }
                case "crusader kings iii":
                    {
                        return ProjectType.CrusaderKingsIII;
                    }
                case "europa universalis iv":
                    {
                        return ProjectType.EuropaUniversalisIV;
                    }
                case "victoria 3":
                    {
                        return ProjectType.VictoriaIII;
                    }

                default:
                    {
                        MessageBox.Show("We were unable to find the game the .mod file belongs to, make sure it is placed in a game's folder.");
                        return ProjectType.Undefined;
                    }
            }
        }

        public void LoadModFile(string path)
        {
            fileExplorer.Items.Clear();

            string modFilePath = path;

            string content = File.ReadAllText(path);

            Dictionary<string, string> modFile = ModFileParser.ParseFile(content);
            Title = "HMCE - " + modFile["name"];

            int from = path.IndexOf(paradoxFolder) + paradoxFolder.Length;
            path = path.Substring(from);
            path = path.TrimStart('\\');
            path = path.TrimStart('\\');
            path = path.Substring(0, path.IndexOf('\\'));

            projectType = GetProjectTypeFromName(path);

            string modPath = modFile["path"];

            if (!Directory.Exists(modPath))
            {
                MessageBox.Show("The path specified in the .mod file does not exist.");
                return;
            }

            activeProject = modPath;

            FileExplorerButton root = new FileExplorerButton(modPath, modPath.Substring(modPath.LastIndexOf('/')).TrimStart('/'), FileType.Folder);
            fileExplorer.Items.Add(root);

            MapFolder(modPath, root);

            if (!Directory.Exists(editorDocumentDirectory))
            {
                Directory.CreateDirectory(editorDocumentDirectory);
            }

            if (!File.Exists(projectCache))
            {
                File.Create(projectCache);
            }

            string cache = File.ReadAllText(projectCache);

            List<string> cachedProjects = cache.Split('?').ToList();

            if (cachedProjects.Contains(modFilePath + "|" + path))
            {
                cachedProjects.Remove(modFilePath + "|" + path);
            }

            cachedProjects.Add(modFilePath + "|" + path);

            if (cachedProjects.Count > 10)
            {
                cachedProjects.RemoveAt(0);
            }

            string cacheString = "";

            foreach (string project in cachedProjects)
            {
                cacheString += project;
                cacheString += "?";
            }

            cacheString = cacheString.TrimStart('?');
            cacheString = cacheString.TrimEnd('?');

            File.WriteAllText(projectCache, cacheString);

            GetRecentProjects();
        }

        private void SelectDescriptor_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a .mod file.",
                Filter = "Paradox Interactive Mod Files (*.mod)|*.mod"
            };
            openFileDialog.InitialDirectory = paradoxFolder;
            if (openFileDialog.ShowDialog() == true)
            {
                LoadModFile(openFileDialog.FileName);
            }
        }

        private void MapFolder(string path, FileExplorerButton rootButton)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show("An internal error has occured. Some files may be missing!");
                    continue;
                }

                string content = File.ReadAllText(file);

                switch (projectType)
                {
                    case ProjectType.HeartsOfIronIV:
                        {
                            ScintillaManager.RegisterDocumentUpdate(file, content);
                            break;
                        }
                }

                rootButton.Add(new FileExplorerButton(file, Path.GetFileName(file), FileType.File));
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                if (!Directory.Exists(directory))
                {
                    MessageBox.Show("An internal error has occured. Some files may be missing!");
                    continue;
                }

                FileExplorerButton dirButton = new FileExplorerButton(directory, directory.Substring(directory.LastIndexOf('\\')).TrimStart('\\'), FileType.Folder);

                rootButton.Add(dirButton);
                MapFolder(directory, dirButton);
            }
        }

        private void SaveDocument_Click(object sender, EventArgs e)
        {
            ScintillaManager.SaveDocument();
        }
    }
}
