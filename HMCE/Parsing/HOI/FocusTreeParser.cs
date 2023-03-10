using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HMCE.Parsing.HOI
{
    internal class FocusTreeParser
    {
        private readonly List<FocusToken> tokens;
        private int current = 0;

        private bool IsAtEnd => Peek().type == FocusTokenType.EoF;

        public FocusTreeParser(List<FocusToken> tokens) 
        {
            this.tokens = tokens;
        }

        public List<FocusTreeStatement.Focus> Parse()
        {
            List<FocusTreeStatement.Focus> statements = new List<FocusTreeStatement.Focus>();
            while (!IsAtEnd)
            {
                FocusTreeStatement.Focus focus = FocusDeclaration();
                if (focus != null)
                {
                    statements.Add(focus);
                }
            }

            return statements;
        }

        private FocusTreeStatement.Focus FocusDeclaration()
        {
            if (!Match(FocusTokenType.Focus))
            {
                Advance();
                return null;
            }

            Consume(FocusTokenType.Equal, "Expected '=' after 'focus'.");
            Consume(FocusTokenType.LeftBrace, "Expected '{' to begin focus declaration.");

            List<FocusTreeStatement> statements = new List<FocusTreeStatement>();
            while (!Check(FocusTokenType.RightBrace) && !IsAtEnd)
            {
                FocusTreeStatement stmt = Property();
                if (stmt == null) break;
                statements.Add(stmt);
            }

            Consume(FocusTokenType.RightBrace, "Expected '}' after focus declaration.");

            return new FocusTreeStatement.Focus(statements);
        }

        private FocusTreeStatement Property()
        {
            if (Match(FocusTokenType.Id, FocusTokenType.Icon, FocusTokenType.X, FocusTokenType.Y)) return Assignment();
            else if (Match(FocusTokenType.MutuallyExclusive, FocusTokenType.Prerequisite)) return Relation();

            return null;
        }

        private FocusTreeStatement Assignment()
        {
            FocusToken type = Previous();

            Consume(FocusTokenType.Equal, "Expected '=' for assignment.");

            object value = Advance().value;

            if (!(value is int) && !(value is string))
            {
                return null;
            }

            return new FocusTreeStatement.AssignStatement(type, new FocusTreeStatement.Literal(value));
        }

        private FocusTreeStatement Relation()
        {
            FocusToken type = Previous();

            Consume(FocusTokenType.Equal, "Expected '=' for relation assignment.");
            Consume(FocusTokenType.LeftBrace, "Expected '{' for relation assignment.");

            List<FocusToken> relations = new List<FocusToken>();

            while (!Check(FocusTokenType.RightBrace) && !IsAtEnd)
            {
                Consume(FocusTokenType.Equal, "Expected 'focus' before identifier.");
                Consume(FocusTokenType.Equal, "Expected '=' after 'focus'.");
                relations.Add(Advance());
            }

            Consume(FocusTokenType.RightBrace, "Expected '}' after relation declaration.");

            return new FocusTreeStatement.RelationStatement(type, relations);
        }

        private FocusToken Peek()
        {
            return tokens[current];
        }

        private FocusToken Previous()
        {
            return tokens[current - 1];
        }

        private bool Match(params FocusTokenType[] types)
        {
            foreach (FocusTokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private FocusToken Consume(FocusTokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw new Exception();

            // TODO: Add Error Handling
            return null;
        }

        private bool Check(FocusTokenType tokenType)
        {
            if (IsAtEnd) return false;
            return Peek().type == tokenType;
        }

        private FocusToken Advance()
        {
            if (!IsAtEnd) current++;
            return Previous();
        }
    }
}
