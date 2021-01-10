using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DGen.Common
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };
        
        public static string ToSnakeCase(this string str) 
        {
            Regex pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
            return string.Join("_", pattern.Matches(str)).ToLower();
        }

        public static StreamWriter Indent(this StreamWriter writer, int level = 1)
        {
            for (int i = 0; i < level; i++)
            {
                writer.Write("    ");
            }
            return writer;
        }
    }
}