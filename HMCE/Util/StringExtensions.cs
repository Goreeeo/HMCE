using System.Text.RegularExpressions;

namespace HMCE
{
    public static class StringExtensions
    {
        private static Regex whitespace = new Regex(@"\s+");
        public static string RemoveWhitespace(this string source)
        {
            return whitespace.Replace(source, "");
        }
    }
}
