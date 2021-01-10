using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DGen.Common;
using DGen.Models;

namespace CSharp
{
    public class CSharpWriter : ICodeWriter
    {
        private static readonly Regex UsingRegex = new Regex("using .+");
        private static readonly Regex NsRegex = new Regex("namespace .+");

        public void Write(ClassDeclInfo declInfo, StreamWriter target)
        {
            target.WriteLine("#pragma warning disable all");

            WriteUsings(declInfo, target);
            bool hasNs = WriteNamespace(declInfo, target);

            WriteClassDecl(declInfo, target);
            WriteFields(declInfo, target);
            WriteConstructor(declInfo, target);

            target.WriteLine("}");

            if (hasNs)
                target.WriteLine("}");
        }


        private static void WriteUsings(ClassDeclInfo typeInfo, StreamWriter csharpWriter)
        {
            var usings = new List<string>
            {
                "using UnityEngine;",
                "using Newtonsoft.Json;",
                "using System;",
                "using JetBrains.Annotations;",
                "using System.Collections.Generic;"
            };

            foreach (string s in typeInfo.Options.Where(s => !usings.Contains(s)).Where(s => UsingRegex.IsMatch(s)))
            {
                usings.Add(s + ";");
            }

            foreach (string u in usings)
            {
                csharpWriter.WriteLine(u);
            }
        }

        private static bool WriteNamespace(ClassDeclInfo typeInfo, StreamWriter csharpWriter)
        {
            string ns = null;
            foreach (string s in typeInfo.Options)
            {
                if (NsRegex.IsMatch(s))
                    ns = s;
            }

            if (!string.IsNullOrEmpty(ns))
            {
                csharpWriter.WriteLine(ns);
                csharpWriter.WriteLine("{");
                return true;
            }

            return false;
        }

        private static void WriteClassDecl(ClassDeclInfo typeDeclInfo, StreamWriter csharpWriter)
        {
            if (typeDeclInfo.Docs != null)
            {
                csharpWriter.WriteLine("/// <summary>");
                foreach (string doc in typeDeclInfo.Docs)
                {
                    csharpWriter.Write("/// ");
                    csharpWriter.WriteLine(doc);
                }

                csharpWriter.WriteLine("/// </summary>");
            }

            csharpWriter.WriteLine("[System.Serializable]");
            csharpWriter.Write("public partial");
            csharpWriter.Write(typeDeclInfo.IsStruct ? " struct " : " class ");
            csharpWriter.Write(typeDeclInfo.Name);

            csharpWriter.WriteLine("{");
        }

        private static void WriteFields(ClassDeclInfo typeInfo, StreamWriter cs)
        {
            foreach (var field in typeInfo.Fields)
            {
                string fieldType = field.Type.Replace("?", "");

                string typeResult = TypeMap.GetTypeName(fieldType);

                cs.WriteLine(field.IsNullable ? "[SerializeReference]" : "[SerializeField]");

                string csharpFieldName = "@" + field.Name;

                cs.WriteLine($"[JsonProperty(\"{field.Name.FirstCharToUpper()}\")]");
                cs.Write("[Tooltip(\"");
                cs.Write(string.Join(' ', field.Docs).Trim());
                cs.WriteLine("\")]");
                cs.WriteLine($"private {typeResult} {csharpFieldName};");

                cs.WriteLine("/// <summary>");
                foreach (string doc in field.Docs)
                {
                    cs.Write("/// ");
                    cs.WriteLine(doc);
                }

                cs.WriteLine("/// </summary>");

                if (field.IsNullable)
                    cs.WriteLine("[CanBeNull]");
                cs.WriteLine("[JsonIgnore]");
                cs.WriteLine($"public {typeResult} {field.Name.FirstCharToUpper()}");
                cs.WriteLine(
                    $"{{ get {{ return this.{csharpFieldName}; }} set {{ this.{csharpFieldName} = value;}}}}");
                cs.WriteLine();
            }
        }

        private static void WriteConstructor(ClassDeclInfo typeInfo, StreamWriter cs)
        {
            if (typeInfo.IsStruct == false)
            {
                cs.WriteLine($"public {typeInfo.Name}() {{}}");
            }

            List<FieldDeclInfo> csharpFields = typeInfo.Fields;

            cs.Write("public " + typeInfo.Name + "(");

            for (int i = 0; i < csharpFields.Count; i++)
            {
                FieldDeclInfo field = csharpFields[i];

                string typeResult = TypeMap.GetTypeName(field.Type.Replace("?", ""));

                cs.Write(typeResult);
                cs.Write(" @");
                cs.Write(field.Name);

                if (i < csharpFields.Count - 1)
                    cs.Write(", ");
            }

            cs.WriteLine(")");
            cs.WriteLine("{");

            foreach (FieldDeclInfo field in csharpFields)
            {
                cs.WriteLine("this.@" + field.Name + " = @" + field.Name + ";");
            }

            cs.WriteLine("}");
        }
    }
}