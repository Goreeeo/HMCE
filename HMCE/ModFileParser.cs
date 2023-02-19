using System.Collections.Generic;

namespace HMCE
{
    public static class ModFileParser
    {
        public static Dictionary<string, string> ParseFile(string content)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] pairs = content.Split('=');

            string name = pairs[0];

            for (int i = 1; i < pairs.Length; i++)
            {
                if (pairs[i][0] == '{')
                {
                    name = pairs[i].Split('}')[1];
                    continue;
                }
                else
                {
                    string[] valueName = pairs[i].TrimStart('"').Split('"');

                    string value = valueName[0];

                    name = name.Replace("\n", "");

                    result.Add(name, value);

                    name = valueName[1];
                }
            }

            return result;
        }
    }
}
