using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMCE.Parsing.HOI
{
    internal class ASTPrinter
    {
        private static int indentAmount = 0;

        public static void Print(List<FocusTreeStatement.Focus> statements)
        {
            statements.ForEach((s) => PrintStatement(s));
        }

        private static void PrintStatement(FocusTreeStatement.Focus statement)
        {
            indentAmount = 0;

            Console.WriteLine("Focus");

            indentAmount++;

            foreach (FocusTreeStatement stmt in statement.children)
            {
                if (stmt is FocusTreeStatement.AssignStatement a)
                {
                    PrintAssign(a);
                }
                else if (stmt is FocusTreeStatement.RelationStatement r)
                {
                    PrintRelation(r);
                }
            }
        }

        private static void PrintAssign(FocusTreeStatement.AssignStatement statement)
        {
            Indent();
            Console.WriteLine("Assign");

            indentAmount++;

            Indent();
            Console.WriteLine(statement.left.lexeme.ToString() + " = " + statement.right.value.ToString());

            indentAmount--;
        }

        private static void PrintRelation(FocusTreeStatement.RelationStatement statement)
        {
            Indent();
            Console.WriteLine("Relation | " + statement.parent.lexeme);

            indentAmount++;

            for (int i = 0; i < statement.relations.Count; i++)
            {
                Indent();
                Console.WriteLine(statement.relations[i].lexeme);
            }

            indentAmount--;
        }

        private static void Indent()
        {
            for (int i = 0; i < indentAmount; i++)
            {
                Console.WriteLine('\t');
            }
        }
    }
}
