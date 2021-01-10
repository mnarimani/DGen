using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DGen.Models;

namespace DGen
{
    public static class Parser
    {
        public static readonly Regex SingleLineComment = new Regex(" *//(.*)");

        private const string ClassPattern =
            @".*?(?<classdocs>(/\*.*?\*/)*?) *(?<type>class|struct) +(?<classname>\w[\w\d]*) *{(?<vardecl> *(?<comments>(/\*.*?\*/)*?) *(?<ro>val|var) +(?<varname>\w[\w\d]*) +(?<vartype>[\w\d]+) *; *)*}";

        private static readonly Regex ClassRegex = new Regex(ClassPattern, RegexOptions.Compiled);
        private static readonly Regex OptRegex = new Regex("opt (?<opt>.+?);", RegexOptions.Compiled);

        public static ClassDeclInfo[] Parse(string source)
        {
            source = SingleLineComment.Replace(source, "/*$1*/");
            source = source.Replace("\n", "").Replace("\r", "");

            var opts = new List<string>();
            var optMatches = OptRegex.Matches(source);
            foreach (Match o in optMatches)
            {
                opts.Add(o.Groups["opt"].Value);
            }

            var matches = ClassRegex.Matches(source);
            var structures = new ClassDeclInfo[matches.Count];

            for (var classIdx = 0; classIdx < matches.Count; classIdx++)
            {
                Match classMatch = matches[classIdx];
                var s = new ClassDeclInfo()
                {
                    Name = classMatch.Groups["classname"].Value,
                    Docs = classMatch.Groups["classdocs"].Value.Split("*//*").Select(ProcessComment).ToList()
                };

                int varCount = classMatch.Groups["vardecl"].Captures.Count;

                for (int i = 0; i < varCount; i++)
                {
                    var readonlyStatus = classMatch.Groups["ro"].Captures[i];
                    var varName = classMatch.Groups["varname"].Captures[i];
                    var varType = classMatch.Groups["vartype"].Captures[i];
                    var comments = classMatch.Groups["comments"].Captures[i];

                    s.Fields.Add(new FieldDeclInfo
                    {
                        Name = varName.Value,
                        Type = varType.Value,
                        IsReadonly = readonlyStatus.Value == "val",
                        Docs = comments.Value.Split("*//*").Select(ProcessComment).ToList(),
                    });
                }

                s.Options.AddRange(opts);
                structures[classIdx] = s;
            }

            return structures;
        }
        
        private static string ProcessComment(string doc)
        {
            return doc.Replace("/*", "").Replace("*/", "").Trim();
        }
    }
}