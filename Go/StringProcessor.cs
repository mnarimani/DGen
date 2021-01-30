using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Go
{
    internal static class StringProcessor
    {
        private static readonly List<string> Keywords = Regex.Replace(
            @"
break        default      func         interface    select
case         defer        go           map          struct
chan         else         goto         package      switch
const        fallthrough  if           range        type
continue     for          import       return       var
"
                .Replace("\n", " ")
                .Replace("\r", " "),
            " +", ",").Split(',').ToList();

        public static string RemoveWordFromStart(this string str, string name)
        {
            str = str.Trim();

            if (!str.StartsWith(name, true, CultureInfo.InvariantCulture))
                return str;

            return str.Remove(0, name.Length).Trim();
        }
        
        public static string RemoveWordFromEnd(this string str, string name)
        {
            str = str.Trim();

            if (!str.EndsWith(name, true, CultureInfo.InvariantCulture))
                return str;

            return str.Remove(str.Length - name.Length, name.Length).Trim();
        }

        public static string ToValidTypeName(this string t)
        {
            if (Keywords.Contains(t))
                return t + "_";
            return t;
        }
    }
}