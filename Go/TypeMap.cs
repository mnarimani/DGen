using System.Text.RegularExpressions;
using DGen.Common;

namespace Go
{
    internal static class TypeMap
    {
        public static string GetTypeName(string type)
        {
            type = type.Trim();

            string typeResult;

            if (GetSimpleTypeName(type, out typeResult))
                return typeResult;

            if (MatchMap(type, out typeResult))
                return typeResult;

            if (MatchList(type, out typeResult))
                return typeResult;

            if (MatchArray(type, out typeResult))
                return typeResult;

            return type;
        }

        private static bool MatchArray(string type, out string typeResult)
        {
            Match arrayMatch = TypeRegexes.ArrayRegex.Match(type);
            if (!arrayMatch.Success)
            {
                typeResult = default;
                return false;
            }

            string nested = GetTypeName(arrayMatch.Groups["type"].Value);
            typeResult = $"[]{nested}";
            return true;
        }

        private static bool MatchList(string type, out string typeResult)
        {
            Match listMatch = TypeRegexes.ListRegex.Match(type);
            if (!listMatch.Success)
            {
                typeResult = default;
                return false;
            }

            string nested = GetTypeName(listMatch.Groups["type"].Value);
            typeResult = $"[]{nested}";
            return true;
        }

        private static bool MatchMap(string type, out string typeResult)
        {
            Match mapMatch = TypeRegexes.MapRegex.Match(type);

            if (!mapMatch.Success)
            {
                typeResult = default;
                return false;
            }

            string keyType = GetTypeName(mapMatch.Groups["key"].Value);
            string valueType = GetTypeName(mapMatch.Groups["value"].Value);

            typeResult = $"map[{keyType}]{valueType}";
            return true;
        }

        private static bool GetSimpleTypeName(string type, out string typeResult)
        {
            switch (type)
            {
                case "int":
                    typeResult = "int";
                    break;
                case "long":
                    typeResult = "int64";
                    break;
                case "float":
                    typeResult = "float32";
                    break;
                case "double":
                    typeResult = "float64";
                    break;
                case "bool":
                    typeResult = "bool";
                    break;
                case "string":
                    typeResult = "string";
                    break;
                default:
                    typeResult = default;
                    return false;
            }

            return true;
        }
    }
}