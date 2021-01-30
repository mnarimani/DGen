using System;
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
            @".*?(?<classdocs>(/\*.*?\*/)*?) *(?<type>class|struct) +(?<classname>\w[\w\d]*) *{ *(?<vardecl> *(?<comments>(/\*.*?\*/)*?) *(?<attributes>(\<{10}.+?\>{10})*?) *(?<varname>\w[\w\d]*) +(?<vartype>[\w\d<>\[\],]+) *; *)* *}";

        private static readonly Regex ClassRegex = new Regex(ClassPattern, RegexOptions.Compiled);
        private static readonly Regex OptRegex = new Regex("opt (?<opt>.+?);", RegexOptions.Compiled);
        private static readonly Regex AttributeDetector = new Regex("@(\\S+) *\n*");
        private static readonly Regex ConvertedAttributeDetector = new Regex("\\<{10}(?<attributeValue>\\S+?)\\>{10}");

        public static ClassDeclInfo[] Parse(string source)
        {
            source = SingleLineComment.Replace(source, "/*$1*/");
            source = AttributeDetector.Replace(source, "<<<<<<<<<<$1>>>>>>>>>>");
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
                    Docs = classMatch.Groups["classdocs"].Value.Split("*//*").Select(ProcessComment).ToList(),
                    IsStruct = classMatch.Groups["type"].Value == "struct"
                };

                int varCount = classMatch.Groups["vardecl"].Captures.Count;

                for (int i = 0; i < varCount; i++)
                {
                    string varName = classMatch.Groups["varname"].Captures[i].Value;
                    string varType = classMatch.Groups["vartype"].Captures[i].Value;
                    string comments = classMatch.Groups["comments"].Captures[i].Value;
                    string attributeString = classMatch.Groups["attributes"].Captures[i].Value;
                    
                    var f = new FieldDeclInfo
                    {
                        Name = varName,
                        Type = varType,
                        Docs = comments.Split("*//*").Select(ProcessComment).ToList(),
                    };
                    
                    var attrMatches = ConvertedAttributeDetector.Matches(attributeString);
                    foreach (Match m in attrMatches)
                    {
                        f.Attributes.Add(m.Groups["attributeValue"].Value);
                    }

                    s.Fields.Add(f);
                    
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