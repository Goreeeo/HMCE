using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMCE.Parsing.HOI
{
    internal abstract class FocusTreeStatement
    {
        public class Focus : FocusTreeStatement
        {
            public List<FocusTreeStatement> children;

            public Focus(List<FocusTreeStatement> children)
            {
                this.children = children;
            }
        }

        public class RelationStatement : FocusTreeStatement
        {
            public FocusToken parent;
            public List<FocusToken> relations = new List<FocusToken>();

            public RelationStatement(FocusToken parent, List<FocusToken> relations)
            {
                this.parent = parent;
                this.relations = relations;
            }
        }

        public class Literal : FocusTreeStatement
        {
            public object value;

            public Literal(object value)
            {
                this.value = value;
            }
        }

        public class AssignStatement : FocusTreeStatement
        {
            public FocusToken left;
            public Literal right;

            public AssignStatement(FocusToken left, Literal right)
            {
                this.left = left;
                this.right = right;
            }
        }
    }
}
