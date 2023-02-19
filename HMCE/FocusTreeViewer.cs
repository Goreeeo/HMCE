using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HMCE
{
    public static class FocusTreeViewer
    {

        private class FocusTreeElement : Grid
        {
            public TextBlock focusName;
            public Image focusImage;

            public Focus focus;

            public FocusTreeElement(string name, string imageUri, Focus focus)
            {
                this.focus = focus;

                focusImage = new Image
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(0, 0, 0, 25),
                    Source = DDSUtil.ReadDDS(imageUri)
                };
                Children.Add(focusImage);

                focusName = new TextBlock
                {
                    Height = 60,
                    Width = 100,
                    Margin = new Thickness(0, 75, 0, 0),
                    Padding = new Thickness(0, 0, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                    Text = name,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    TextAlignment = TextAlignment.Center

                };
                Children.Add(focusName);

                Margin = new Thickness(0, 0, focus.x, focus.y);
            }
        }

        private static UniformGrid focusTreeViewer;

        public static List<Focus> rootFocuses = new List<Focus>();

        public static void Init(UniformGrid viewer)
        {
            focusTreeViewer = viewer;
        }

        public static void AddFocus(Focus focus)
        {
            string image = "undefined";

            if (Directory.Exists(Path.Combine(MainWindow.activeProject, "interface")))
            {
                string[] files = Directory.GetFiles(Path.Combine(MainWindow.activeProject, "interface"));

                foreach (string file in files)
                {
                    if (!file.EndsWith(".gfx")) continue;

                    string content = File.ReadAllText(file);

                    if (content.Contains(focus.graphic))
                    {
                        int index = content.IndexOf(focus.graphic) + focus.graphic.Length;
                        content = content.Substring(index);

                        index = content.IndexOf("textureFile") + "textureFile".Length;
                        content = content.Substring(index);

                        index = content.IndexOf('=') + 1;
                        content = content.Substring(index);

                        index = content.IndexOf('}');
                        content = content.Substring(0, index);



                        image = Path.Combine(MainWindow.activeProject, content.RemoveWhitespace());
                    }
                }
            }

            focusTreeViewer.Children.Add(new FocusTreeElement(focus.focusId, image, focus));
        }
    }
}
