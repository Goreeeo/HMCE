using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HMCE
{
    public static class IntExtensions
    {
        private static Regex whitespace = new Regex(@"\s+");
        public static int GetWhitespaceOffset(this int index, string original, string noWhitespace)
        {
            int indexRet = 0;

            for (int i = 0; i < index + indexRet; i++)
            {
                if (whitespace.IsMatch(original[i].ToString()))
                {
                    indexRet++;
                }
            }

            return indexRet;
        }
    }
}
