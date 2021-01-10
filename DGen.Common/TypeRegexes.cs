using System.Text.RegularExpressions;

namespace DGen.Common
{
    public static class TypeRegexes
    {
        public static readonly Regex MapRegex = new Regex(@"^map *< *(?<key>[\w<,> \.\[\]]+\??) *, *(?<value>[\w<,> \.\[\]]+\??) *>");
        public static readonly Regex ListRegex = new Regex(@"^list *< *(?<type>[\w<,> \.\[\]]+\??) *> *");
        public static readonly Regex ArrayRegex = new Regex(@"^(?<type>[\w<,> \.\[\]]+\??)\[\] *");
    }
}