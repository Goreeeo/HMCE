using HMCE.Lexers;
using ScintillaNET;
using System.Collections.Generic;
using System.IO;
using Lexer = HMCE.Lexers.Lexer;

namespace HMCE
{
    public static class ScintillaManager
    {
        private static Scintilla scintilla;

        private static HOILexer hoi;

        private static readonly Dictionary<MainWindow.ProjectType, Lexer> lexers = new Dictionary<MainWindow.ProjectType, Lexer>();

        public static void Init(Scintilla scintilla)
        {
            ScintillaManager.scintilla = scintilla;
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.Styles[Style.Default].BackColor = System.Drawing.Color.FromArgb(255, 8, 0, 18);
            scintilla.Styles[Style.Default].ForeColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.StyleClearAll();

            scintilla.Styles[Lexer.StyleDefault].ForeColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            scintilla.Styles[Lexer.StyleBraces].ForeColor = System.Drawing.Color.FromArgb(255, 200, 130, 232);
            scintilla.Styles[Lexer.StyleColorFormatting].ForeColor = System.Drawing.Color.FromArgb(255, 240, 136, 67);
            scintilla.Styles[Lexer.StyleComment].ForeColor = System.Drawing.Color.FromArgb(255, 119, 138, 118);
            scintilla.Styles[Lexer.StyleEffect].ForeColor = System.Drawing.Color.FromArgb(255, 166, 65, 63);
            scintilla.Styles[Lexer.StyleError].ForeColor = System.Drawing.Color.FromArgb(255, 199, 27, 24);
            scintilla.Styles[Lexer.StyleEscape].ForeColor = System.Drawing.Color.FromArgb(255, 230, 210, 99);
            scintilla.Styles[Lexer.StyleFormatting].ForeColor = System.Drawing.Color.FromArgb(255, 90, 161, 84);
            scintilla.Styles[Lexer.StyleNumber].ForeColor = System.Drawing.Color.FromArgb(255, 196, 139, 69);
            scintilla.Styles[Lexer.StyleOperator].ForeColor = System.Drawing.Color.FromArgb(255, 119, 230, 140);
            scintilla.Styles[Lexer.StyleString].ForeColor = System.Drawing.Color.FromArgb(255, 47, 97, 56);
            scintilla.Styles[Lexer.StyleTrigger].ForeColor = System.Drawing.Color.FromArgb(255, 58, 79, 171);
            scintilla.Styles[Lexer.StyleVariable].ForeColor = System.Drawing.Color.FromArgb(70, 111, 145, 255);
            scintilla.Styles[Lexer.StyleKeyword].ForeColor = System.Drawing.Color.FromArgb(70, 104, 237, 97);
            scintilla.Styles[Lexer.StyleParameter].ForeColor = System.Drawing.Color.FromArgb(70, 234, 125, 97);

            hoi = new HOILexer(scintilla);
            lexers.Add(MainWindow.ProjectType.HeartsOfIronIV, hoi);
        }

        public static void SaveDocument()
        {
            string path = lexers[MainWindow.projectType].currentPath;

            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, scintilla.Text);
        }

        public static void SetDocument(string path)
        {
            lexers[MainWindow.projectType].SetDocument(path);
        }

        public static Scintilla GetScintilla()
        {
            return scintilla;
        }

        public static void RegisterDocumentUpdate(string path, string content)
        {
            lexers[MainWindow.projectType].RegisterDocumentUpdate(path, content);
        }
    }
}
