using System.Collections.Generic;
using System.IO;

namespace HMCE.Lexers
{
    public class HOILexer : Lexer
    {
        private static readonly string focusPath = Path.Combine(MainWindow.activeProject, "common", "national_focus");

        private List<ParameteredKeyword> effects = new List<ParameteredKeyword>()
        {
            new ParameteredKeyword("add_dynamic_modifier", "modifier")
        };
        protected override List<ParameteredKeyword> Effects
        {
            get => effects;
            set => effects = value;
        }

        private List<ParameteredKeyword> triggers = new List<ParameteredKeyword>()
        {

        };
        protected override List<ParameteredKeyword> Triggers
        {
            get => triggers;
            set => triggers = value;
        }

        private List<ParameteredKeyword> keywords = new List<ParameteredKeyword>()
        {

        };
        protected override List<ParameteredKeyword> Keywords
        {
            get => keywords;
            set => keywords = value;
        }

        public HOILexer(ScintillaNET.Scintilla scintilla) : base(scintilla)
        {

        }

        protected override void OnDocumentUpdate(string path, string content)
        {
            /*if (path.Contains(focusPath))
            {
                FocusCollection.Clear();

                while (content.Contains("focus"))
                {
                    int index = content.IndexOf("focus");
                    int oIndex = content.IndexOf("focus");
                    string focus = content.Substring(index);

                    string noWhitespace = focus.RemoveWhitespace();

                    index = noWhitespace.IndexOf("}focus") > 0 ? noWhitespace.IndexOf("}focus") : noWhitespace.IndexOf("}}");
                    index += index.GetWhitespaceOffset(focus, noWhitespace);
                    focus = focus.Substring(0, index);

                    string focusNW = focus.RemoveWhitespace();

                    if (!focusNW.StartsWith("focus={"))
                    {
                        content = content.Insert(oIndex + 2, "k");
                        continue;
                    }

                    string id = "";

                    if (focusNW.Contains("id="))
                    {
                        index = focus.IndexOf("id") + "id".Length;
                        id = focus.Substring(index);

                        index = id.IndexOf("=") + 1;
                        id = id.Substring(index);

                        index = id.IndexOf('\n') >= 0 ? id.IndexOf('\n') : (id.IndexOf('\r') >= 0 ? id.IndexOf('\r') : id.IndexOf(' '));
                        id = id.Substring(0, index).RemoveWhitespace();
                    }

                    List<string> mutuallyExclusiveFoci = new List<string>();

                    if (focusNW.Contains("mutually_exclusive="))
                    {
                        index = focus.IndexOf("mutually_exclusive") + "mutually_exclusive".Length;
                        string mutuallyExclusive = focus.Substring(index);

                        index = mutuallyExclusive.IndexOf("=") + 1;
                        mutuallyExclusive = mutuallyExclusive.Substring(index);

                        index = mutuallyExclusive.IndexOf('}');
                        mutuallyExclusive = mutuallyExclusive.Substring(0, index);

                        while (mutuallyExclusive.Contains("focus"))
                        {
                            oIndex = mutuallyExclusive.IndexOf("focus") + "focus".Length;
                            index = mutuallyExclusive.IndexOf("focus") + "focus".Length;
                            string focusId = mutuallyExclusive.Substring(index);

                            index = focusId.IndexOf("=") + 1;
                            focusId = focusId.Substring(index);

                            index = focusId.IndexOf('\n') >= 0 ? focusId.IndexOf('\n') : (focusId.IndexOf('\r') >= 0 ? focusId.IndexOf('\r') : focusId.IndexOf(' '));
                            mutuallyExclusiveFoci.Add(focusId.Substring(0, index).RemoveWhitespace());
                            mutuallyExclusive.Insert(oIndex + 2, "k");
                        }
                    }

                    List<string> prerequisiteFoci = new List<string>();

                    if (focusNW.Contains("prerequisite="))
                    {
                        index = focus.IndexOf("prerequisite") + "prerequisite".Length;
                        string prerequisite = focus.Substring(index);

                        index = prerequisite.IndexOf("=") + 1;
                        prerequisite = prerequisite.Substring(index);

                        index = prerequisite.IndexOf('}');
                        prerequisite = prerequisite.Substring(0, index);

                        while (prerequisite.Contains("focus"))
                        {
                            oIndex = prerequisite.IndexOf("focus") + "focus".Length;
                            index = prerequisite.IndexOf("focus") + "focus".Length;
                            string focusId = prerequisite.Substring(index);

                            index = focusId.IndexOf("=") + 1;
                            focusId = focusId.Substring(index);

                            index = focusId.IndexOf('\n') >= 0 ? focusId.IndexOf('\n') : (focusId.IndexOf('\r') >= 0 ? focusId.IndexOf('\r') : focusId.IndexOf(' '));
                            prerequisiteFoci.Add(focusId.Substring(0, index).RemoveWhitespace());
                            prerequisite.Insert(oIndex + 2, "k");
                        }
                    }

                    string icon = "";

                    if (focusNW.Contains("icon="))
                    {
                        index = focus.IndexOf("icon") + "icon".Length;
                        icon = focus.Substring(index);

                        index = icon.IndexOf("=") + 1;
                        icon = icon.Substring(index);

                        index = icon.IndexOf('\n') >= 0 ? icon.IndexOf('\n') : (icon.IndexOf('\r') >= 0 ? icon.IndexOf('\r') : icon.IndexOf(' '));
                        icon = icon.Substring(0, index).RemoveWhitespace();
                    }

                    string relPosId = "";

                    if (focusNW.Contains("relative_focus_id="))
                    {
                        index = focus.IndexOf("relative_focus_id") + "relative_focus_id".Length;
                        relPosId = focus.Substring(index);

                        index = relPosId.IndexOf("=") + 1;
                        relPosId = relPosId.Substring(index);

                        index = relPosId.IndexOf('\n') >= 0 ? relPosId.IndexOf('\n') : (relPosId.IndexOf('\r') >= 0 ? relPosId.IndexOf('\r') : relPosId.IndexOf(' '));
                        relPosId = relPosId.Substring(0, index).RemoveWhitespace();
                    }

                    string x = "";

                    if (focusNW.Contains("x="))
                    {
                        index = focus.IndexOf("x") + "x".Length;
                        x = focus.Substring(index);

                        index = x.IndexOf("=") + 1;
                        x = x.Substring(index);

                        index = x.IndexOf('\n') >= 0 ? x.IndexOf('\n') : (x.IndexOf('\r') >= 0 ? x.IndexOf('\r') : x.IndexOf(' '));
                        x = x.Substring(0, index).RemoveWhitespace();
                    }

                    string y = "";

                    if (focusNW.Contains("y="))
                    {
                        index = focus.IndexOf("y") + "y".Length;
                        y = focus.Substring(index);

                        index = y.IndexOf("=") + 1;
                        y = y.Substring(index);

                        index = y.IndexOf('\n') >= 0 ? y.IndexOf('\n') : (y.IndexOf('\r') >= 0 ? y.IndexOf('\r') : y.IndexOf(' '));
                        y = y.Substring(0, index).RemoveWhitespace();
                    }

                    Focus focusObj = new Focus(id, icon, mutuallyExclusiveFoci, prerequisiteFoci, relPosId, x, y);
                    FocusCollection.Add(focusObj);

                    content = content.Insert(oIndex + 2, "k");
                }

                FocusCollection.OnCompleteRegister();
            }*/
        }
    }
}
