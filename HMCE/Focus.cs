using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HMCE
{
    public static class FocusCollection
    {
        private static readonly List<Focus> focusList = new List<Focus>();

        public static void Add(Focus focus)
        {
            focusList.Add(focus);
        }

        public static void Remove(Focus focus)
        {
            focusList.Remove(focus);
        }

        public static void Clear()
        {
            focusList.Clear();
        }

        public static bool Contains(Focus focus)
        {
            return focusList.Contains(focus);
        }

        public static Focus Get(string focus)
        {
            return focusList.Find((f) => f.focusId == focus);
        }

        public static List<Focus> GetAll(string[] foci)
        {
            return focusList.FindAll(
                delegate(Focus focus) {
                    foreach (string focusId in foci)
                    {
                        if (focusId == focus.focusId)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            );
        }

        public static void OnCompleteRegister()
        {
            focusList.ForEach(
                delegate (Focus focus)
                {
                    if (focus.prerequisite.Any())
                    {
                        focus.prerequisite.ForEach(
                            delegate (string focusId)
                            {
                                Focus preFocus = Get(focusId);
                                focus.prerequisiteFoci.Add(preFocus);
                                preFocus.children.Add(focus);
                            }
                        );
                    } 
                    else
                    {
                        FocusTreeViewer.rootFocuses.Add(focus);
                    }

                    focus.mutuallyExclusive.ForEach((focusId) => focus.mutuallyExclusiveFoci.Add(Get(focusId)));

                    if (focus.relativePositionFocus != null)
                    {
                        focus.relativePositionFocus = Get(focus.relativePositionId);
                    }

                    focus.x += focus.relativePositionFocus != null ? focus.relativePositionFocus.x : 0;
                    focus.y += focus.relativePositionFocus != null ? focus.relativePositionFocus.y : 0;
                }
            );

            focusList.ForEach((focus) => FocusTreeViewer.AddFocus(focus));
        }
    }

    public class Focus
    {
        public string focusId;
        public string graphic;

        public List<string> mutuallyExclusive;
        public List<string> prerequisite;

        public List<Focus> mutuallyExclusiveFoci;
        public List<Focus> prerequisiteFoci;
        public List<Focus> children;

        public string relativePositionId;
        public Focus relativePositionFocus;
        public int x, y;

        public Focus(string id, string graphic, List<string> mutuallyExclusive, List<string> prerequisite, string relativePositionId, string x, string y)
        {
            focusId = id;
            this.graphic = graphic;
            this.mutuallyExclusive = mutuallyExclusive ?? new List<string>();
            this.prerequisite = prerequisite ?? new List<string>();
            this.children = children ?? new List<Focus>();
            this.relativePositionId = relativePositionId;
            int.TryParse(x, out this.x);
            int.TryParse(y, out this.y);
            FocusCollection.Add(this);
        }
    }
}
