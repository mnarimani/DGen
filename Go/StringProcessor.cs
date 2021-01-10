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

        public static string RemoveMemberName(this string doc, string name)
        {
            doc = doc.Trim();

            if (!doc.StartsWith(name, true, CultureInfo.InvariantCulture))
                return doc;

            return doc.Remove(0, name.Length).Trim();
        }

        public static string ToValidTypeName(this string t)
        {
            if (Keywords.Contains(t))
                return t + "_";
            return t;
        }
    }
}