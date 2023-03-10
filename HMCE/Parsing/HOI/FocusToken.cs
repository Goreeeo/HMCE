using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMCE.Parsing.HOI
{
    internal enum FocusTokenType
    {
        Focus = 0,
        Id = 1,
        MutuallyExclusive = 2,
        Prerequisite = 3,
        Icon = 4,
        X = 5,
        Y = 6,

        Equal = 7,

        LeftBrace = 8,
        RightBrace = 9,

        Identifier = 10,
        Int = 11,
        EoF = 12
    }

    internal class FocusToken
    {
        public FocusTokenType type;
        public string lexeme;
        public object value;
        public int line;

        public FocusToken(FocusTokenType type, string lexeme, object value, int line)
        {
            this.type = type; 
            this.lexeme = lexeme;
            this.value = value;
            this.line = line;
        }
    }
}
