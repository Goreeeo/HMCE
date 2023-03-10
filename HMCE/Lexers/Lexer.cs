using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HMCE.Lexers
{
    public abstract class Lexer
    {
        protected struct ParameteredKeyword
        {
            public string name;
            public string[] parameters;

            public ParameteredKeyword(string name, params string[] parameters)
            {
                this.name = name;
                this.parameters = parameters;
            }
        }

        protected class Document
        {
            public string path;
            public List<Variable> variables;
            public string content;

            public Document(string path, string content)
            {
                this.path = path;
                this.content = content;
                variables = new List<Variable>();
            }
        }

        protected class Variable
        {
            public string name;
            public int count;
            public Document document;

            public Variable(string name, int count, Document document)
            {
                this.name = name;
                this.count = count;
                this.document = document;
            }
        }

        public const int StyleDefault = 0,
            StyleEffect = 1,
            StyleTrigger = 2,
            StyleNumber = 3,
            StyleString = 4,
            StyleBraces = 5,
            StyleOperator = 6,
            StyleComment = 7,
            StyleError = 8,
            StyleColorFormatting = 9,
            StyleFormatting = 10,
            StyleEscape = 11,
            StyleVariable = 12,
            StyleKeyword = 13,
            StyleParameter = 14;

        private const int STATE_UNKNOWN = 0,
            STATE_IDENTIFIER = 1,
            STATE_NUMBER = 2,
            STATE_STRING = 3;

        private static readonly List<char> Operators = new List<char>()
        {
            '*', '+', '-', '/', '%'
        };

        private static readonly List<char> Numbers = new List<char>()
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '8', '-', '.'
        };

        private static readonly List<char> EscapeSequences = new List<char>()
        {
            'n'
        };

        private static readonly List<char> ColorFormattingSequences = new List<char>()
        {
            '!', 'C', 'L', 'W', 'B', 'G', 'R', 'b', 'g', 'Y', 'H', 'T', 'O', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 't'
        };

        private static readonly List<char> FormattingSequences = new List<char>()
        {
            '%', '*', '^', '=', '0', '1', '2', '3', '+', '-'
        };

        protected abstract List<ParameteredKeyword> Keywords { get; set; }
        protected abstract List<ParameteredKeyword> Effects { get; set; }
        protected abstract List<ParameteredKeyword> Triggers { get; set; }

        private readonly List<Variable> UserVariables = new List<Variable>();
        private readonly Dictionary<string, Document> Documents = new Dictionary<string, Document>();

        public bool currentInUse = false;

        public string currentPath = "";

        private string autoShowListString;

        public Lexer(Scintilla scintilla)
        {
            scintilla.StyleNeeded += (s, se) =>
            {
                Style(scintilla, scintilla.GetEndStyled(), se.Position, currentPath, false);
            };

            string autoShowListString = "";

            List<string> autoShowList = BuildAutoCompleteList();

            foreach (string element in autoShowList)
            {
                autoShowListString += element.ToLower();
                autoShowListString += ';';
            }

            autoShowListString.TrimEnd(';');

            scintilla.CharAdded += CharAdded;
        }

        public void RegisterDocumentUpdate(string path, string content)
        {
            if (!Documents.ContainsKey(path))
            {
                Documents.Add(path, new Document(path, content));
            }
            else
            {
                Documents[path].content = content;
            }

            for (int i = 0; i < Documents[path].variables.Count; i++)
            {
                Documents[path].variables[i].count = 0;
            }

            while (content.Contains("set_variable"))
            {
                int index = content.IndexOf("set_variable") + "set_variable".Length;

                string var = content.Substring(index);

                if (!var.Contains("{") || !var.Contains("}") || !var.Contains("="))
                {
                    content = content.Insert(index - 1, "f");
                    continue;
                }

                int varAssignmentIndex = var.IndexOf('{') + 1;

                var = var.Substring(varAssignmentIndex);
                var = var.Substring(0, var.IndexOf('}'));
                var = var.TrimEnd('}');

                string[] varAssignment = var.Split('=');

                string toInsert = varAssignment[0];

                toInsert = toInsert.Replace("1", "");
                toInsert = toInsert.Replace("2", "");
                toInsert = toInsert.Replace("3", "");
                toInsert = toInsert.Replace("4", "");
                toInsert = toInsert.Replace("5", "");
                toInsert = toInsert.Replace("6", "");
                toInsert = toInsert.Replace("7", "");
                toInsert = toInsert.Replace("8", "");
                toInsert = toInsert.Replace("9", "");
                toInsert = toInsert.Replace("0", "");
                toInsert = toInsert.Replace(".", "");
                toInsert = toInsert.Replace(" ", "");

                AddVariable(Documents[path], toInsert);

                content = content.Insert(index - 1, "f");
            }

            while (content.Contains("set_temp_variable"))
            {
                int index = content.IndexOf("set_temp_variable") + "set_temp_variable".Length;

                string var = content.Substring(index);

                int varAssignmentIndex = var.IndexOf('{') + 1;

                var = var.Substring(varAssignmentIndex);
                var = var.Substring(0, var.IndexOf('}'));
                var = var.TrimEnd('}');

                string[] varAssignment = var.Split('=');

                string toInsert = varAssignment[0];

                toInsert = toInsert.Replace("1", "");
                toInsert = toInsert.Replace("2", "");
                toInsert = toInsert.Replace("3", "");
                toInsert = toInsert.Replace("4", "");
                toInsert = toInsert.Replace("5", "");
                toInsert = toInsert.Replace("6", "");
                toInsert = toInsert.Replace("7", "");
                toInsert = toInsert.Replace("8", "");
                toInsert = toInsert.Replace("9", "");
                toInsert = toInsert.Replace("0", "");
                toInsert = toInsert.Replace(".", "");
                toInsert = toInsert.Replace(" ", "");

                AddVariable(Documents[path], toInsert);

                content = content.Insert(index - 1, "f");
            }

            for (int i = 0; i < Documents[path].variables.Count; i++)
            {
                RefreshVar(Documents[path].variables[i]);
            }

            OnDocumentUpdate(path, content);
        }

        protected abstract void OnDocumentUpdate(string path, string content);

        public void SetDocument(string path)
        {
            currentPath = path;
        }

        private void AddVariable(Document doc, string name)
        {
            Variable v = UserVariables.Find((var) => var.name == name);
            if (v == null)
            {
                Variable nV = new Variable(name, 1, doc);
                UserVariables.Add(nV);
                doc.variables.Add(nV);
                return;
            }
            v.count++;
        }

        private void RefreshVar(Variable var)
        {
            if (var.count <= 0)
            {
                UserVariables.Remove(var);
            }
        }

        public void RemoveVariable(string name)
        {
            Variable v = UserVariables.Find((var) => var.name == name);
            if (v == null)
            {
                return;
            }
            v.count--;
            if (v.count <= 0)
            {
                UserVariables.Remove(v);
            }
        }

        private List<string> BuildAutoCompleteList()
        {
            List<string> list = new List<string>();

            Keywords.ForEach((k) => list.Add(k.name));
            Effects.ForEach((e) => list.Add(e.name));
            Triggers.ForEach((t) => list.Add(t.name));
            UserVariables.ForEach((v) => list.Add(v.name));

            return list.OrderBy(m => m).ToList();
        }

        private void Style(Scintilla scintilla, int startPos, int endPos, string path, bool fullDoc = false)
        {
            RegisterDocumentUpdate(path, scintilla.Text);

            startPos = fullDoc ? 0 : scintilla.Lines[scintilla.LineFromPosition(startPos)].Position;
            endPos = fullDoc ? (scintilla.Lines[scintilla.Lines.Count].EndPosition - 1) : endPos;

            int style, length = 0, state = STATE_UNKNOWN;

            bool SINGLE_LINE_COMMENT;
            void ClearState() { length = state = STATE_UNKNOWN; }
            void DefaultStyle()
            {
                scintilla.SetStyling(1, StyleDefault);
            }

            int StyleUntilEndOfLine(int inPosition, int inStyle)
            {
                int len = scintilla.Lines[scintilla.LineFromPosition(inPosition)].EndPosition - inPosition;

                scintilla.SetStyling(len, inStyle);

                return --len;
            }

            scintilla.StartStyling(startPos);
            {
                for (; startPos < endPos; startPos++)
                {
                    char c = scintilla.Text[startPos];
                    if ((state == STATE_UNKNOWN) && c == ' ') { DefaultStyle(); continue; }

                    char d = ((startPos + 1) < scintilla.Text.Length) ? scintilla.Text[startPos + 1] : '\0';
                    if (state == STATE_UNKNOWN)
                    {
                        bool bString = c == '"',
                            bNegativeNumber = (c == '-') && char.IsDigit(d),
                            bFraction = (c == '.') && char.IsDigit(d);

                        SINGLE_LINE_COMMENT = c == '#';

                        if ((c == '{') || (c == '}'))
                        {
                            scintilla.SetStyling(1, StyleBraces);
                        }
                        else if (char.IsLetter(c))
                        {
                            state = STATE_IDENTIFIER;
                        }
                        else if (bString)
                        {
                            scintilla.SetStyling(1, StyleString);

                            state = STATE_STRING;
                        }
                        else if (char.IsDigit(c) || bNegativeNumber || bFraction)
                        {
                            state = STATE_NUMBER;
                        }
                        else if (SINGLE_LINE_COMMENT)
                        {
                            if (SINGLE_LINE_COMMENT)
                            {
                                startPos += StyleUntilEndOfLine(startPos, StyleComment);
                            }
                        }
                        else if (Operators.Contains(c))
                        {
                            scintilla.SetStyling(1, StyleOperator);
                        }
                        else
                        {
                            DefaultStyle();
                        }

                        continue;
                    }

                    length++;

                    switch (state)
                    {
                        case STATE_IDENTIFIER:
                            {
                                string identifier = scintilla.GetWordFromPosition(startPos);

                                style = StyleDefault;

                                startPos += identifier.Length - 2;

                                if (Effects.FindIndex((e) => e.name == identifier) >= 0) { style = StyleEffect; }
                                else if (Triggers.FindIndex((t) => t.name == identifier) >= 0) { style = StyleTrigger; }
                                else if (Keywords.FindIndex((k) => k.name == identifier) >= 0) { style = StyleKeyword; }
                                else if (UserVariables.Find((v) => v.name == identifier) is Variable) { style = StyleVariable; }

                                ClearState();

                                scintilla.SetStyling(identifier.Length, style);

                                break;
                            }
                        case STATE_NUMBER:
                            {
                                if (!Numbers.Contains(c))
                                {
                                    scintilla.SetStyling(length, StyleNumber);

                                    ClearState();

                                    startPos--;
                                }

                                break;
                            }
                        case STATE_STRING:
                            {
                                style = StyleString;

                                if (c == '"')
                                {
                                    length = (length < 1) ? 1 : length;

                                    scintilla.SetStyling(length, style);

                                    ClearState();
                                }
                                else if (c == '§' && ColorFormattingSequences.Contains(d))
                                {
                                    length--;

                                    scintilla.SetStyling(length, style);
                                    {
                                        startPos++; length = 0;
                                    }

                                    scintilla.SetStyling(2, StyleColorFormatting);
                                }
                                else if (FormattingSequences.Contains(c))
                                {
                                    length--;

                                    scintilla.SetStyling(1, StyleFormatting);
                                }
                                else if (c == '\\' && EscapeSequences.Contains(d))
                                {
                                    length--;

                                    scintilla.SetStyling(length, style);
                                    {
                                        startPos++; length = 0;
                                    }

                                    scintilla.SetStyling(2, StyleEscape);
                                }

                                break;
                            }
                    }
                }
            }
        }

        private int GetLineIndentation(Scintilla scintilla, int line)
        {
            string text = scintilla.Lines[line].Text;
            int firstCharPos = 0;
            for (firstCharPos = 0; firstCharPos < text.Length; firstCharPos++)
            {
                if (!char.IsWhiteSpace(text[firstCharPos])) break;
            }
            text = text.Substring(0, firstCharPos);
            return text.Count(c => c == '\t') + (int)(Math.Floor((double)text.Count(c => c == ' ') / 4));
        }

        private void SetLineIndentation(Scintilla scintilla, int line, int indentation)
        {
            scintilla.Lines[line].Indentation = indentation;
        }

        private void CharAdded(object sender, CharAddedEventArgs e)
        {
            Scintilla scintilla = (Scintilla)sender;

            if (e.Char == '{')
            {
                int scintillaPos = scintilla.CurrentPosition;

                scintilla.AddText("  }");

                scintilla.AnchorPosition = scintillaPos + 1;
                scintilla.CurrentPosition = scintillaPos + 1;
            }

            int currentPos = scintilla.CurrentPosition;
            int wordStartPos = scintilla.WordStartPosition(currentPos, true);
            int lenEntered = currentPos - wordStartPos;

            List<string> autoShowList = BuildAutoCompleteList();
            List<string> autoShowListLower = new List<string>();
            autoShowList.Sort();
            autoShowList.ForEach((l) => autoShowListLower.Add(l.ToLower()));

            scintilla.TargetStart = wordStartPos;
            scintilla.TargetEnd = currentPos;

            if (autoShowListLower.Contains(scintilla.TargetText.ToLower()))
            {
                scintilla.ReplaceTarget(autoShowList.Where((s) => s.ToLower() == scintilla.TargetText.ToLower()).First());
                scintilla.AnchorPosition = scintilla.WordEndPosition(currentPos, true);
                scintilla.CurrentPosition = scintilla.WordEndPosition(currentPos, true);

                void FormatParameters(ParameteredKeyword keyword)
                {
                    if (keyword.parameters.Length > 0)
                    {
                        int tabCount = GetLineIndentation(scintilla, scintilla.CurrentLine);
                        scintilla.AddText(" = {\n");
                        foreach (string param in keyword.parameters)
                        {
                            SetLineIndentation(scintilla, scintilla.CurrentLine, tabCount);
                            scintilla.AddText($"\t{param} = \n");
                        }
                        SetLineIndentation(scintilla, scintilla.CurrentLine, tabCount);
                        scintilla.AddText("}");
                    }
                    else
                    {
                        scintilla.AddText(" = ");
                    }
                }

                int index = Effects.FindIndex((ef) => ef.name.ToLower() == scintilla.TargetText.ToLower());
                if (index >= 0)
                {
                    FormatParameters(Effects[index]);
                    return;
                }
                index = Triggers.FindIndex((t) => t.name.ToLower() == scintilla.TargetText.ToLower());
                if (index >= 0)
                {
                    FormatParameters(Triggers[index]);
                    return;
                }
                index = Keywords.FindIndex((k) => k.name.ToLower() == scintilla.TargetText.ToLower());
                if (index >= 0)
                {
                    FormatParameters(Keywords[index]);
                    return;
                }
            }

            scintilla.AutoCShow(lenEntered, autoShowListString);
        }
    }
}
