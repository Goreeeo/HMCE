using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HMCE.Parsing.HOI
{
    internal class FocusTreeLexer
    {
        private readonly List<FocusToken> tokens = new List<FocusToken>();
        private readonly string input;

        private int start = 0;
        private int current = 0;
        private int line = 1;

        private static readonly Dictionary<string, FocusTokenType> keywords = new Dictionary<string, FocusTokenType>()
        {
            { "focus", FocusTokenType.Focus },
            { "id", FocusTokenType.Id },
            { "mutually_exclusive", FocusTokenType.MutuallyExclusive },
            { "prerequisite", FocusTokenType.Prerequisite },
            { "icon", FocusTokenType.Icon },
            { "x", FocusTokenType.X },
            { "y", FocusTokenType.Y },
        };

        private bool IsAtEnd => current >= input.Length;

        public FocusTreeLexer(string input)
        {
            this.input = input;
        }

        public List<FocusToken> Tokenize()
        {
            while (!IsAtEnd)
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new FocusToken(FocusTokenType.EoF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '{': AddToken(FocusTokenType.LeftBrace); break;
                case '}': AddToken(FocusTokenType.RightBrace); break;
                case '=': AddToken(FocusTokenType.Equal); break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd) Advance();
                    }
                    break;
                default:
                    {
                        if (IsDigit(c))
                        {
                            Number();
                        } else if (IsAlpha(c))
                        {
                            Identifier();
                        }
                        break;
                    }
            }

            
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = input.Substring(start, current - start);

            if (!keywords.TryGetValue(text, out FocusTokenType type))
            {
                type = FocusTokenType.Identifier;
            }
            AddToken(type);
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            AddToken(FocusTokenType.Int,
                double.Parse(input.Substring(start, current - start)));
        }

        private char Advance()
        {
            return input[current++];
        }

        private char Peek() 
        {
            return IsAtEnd ? '\0' : input[current];
        }

        private void AddToken(FocusTokenType type, object literal = null)
        {
            string text = input.Substring(start, current - start);
            tokens.Add(new FocusToken(type, text, literal, line));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd) return false;
            if (input[current] != expected) return false;

            current++;
            return true;
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   (c == '_');
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
    }
}
