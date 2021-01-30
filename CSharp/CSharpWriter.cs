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
        private static readonly Regex NotifyPropertyChangedRegex = new Regex("notify property changed");

        private static readonly Regex FormerlySerializedRegex =
            new Regex("unity=(?<name>\\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex JsonNameRegex =
            new Regex("json=(?<name>\\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MessagePackRegex =
            new Regex("msgpck=(?<name>\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly CmdOptions _options;

        public CSharpWriter(CmdOptions options)
        {
            _options = options;
        }

        public void Write(ClassDeclInfo declInfo, StreamWriter target)
        {
            target.WriteLine("#pragma warning disable all");

            bool notifyPropertyChanged = declInfo.Options.Exists(x => NotifyPropertyChangedRegex.IsMatch(x));

            WriteUsings(declInfo, target, notifyPropertyChanged);
            bool hasNs = WriteNamespace(declInfo, target);

            WriteClassDecl(declInfo, target, notifyPropertyChanged);
            WriteFields(declInfo, target, notifyPropertyChanged);
            WriteConstructor(declInfo, target);

            if (notifyPropertyChanged)
                WriteNotifyPropertyChangedMembers(declInfo, target);

            if (_options.MsgPack)
                WriteMsgPackMethods(declInfo, target);

            target.WriteLine("}");

            if (hasNs)
                target.WriteLine("}");
        }

        private void WriteMsgPackMethods(ClassDeclInfo declInfo, StreamWriter writer)
        {
            void W(string line)
            {
                writer.WriteLine(line);
            }

            string n = declInfo.Name;

            W("private static MessagePackSerializerOptions _msgPackOptions;");
            W("private static void InitMsgPackOptions()");
            W("{");
            W("if (_msgPackOptions == null)");
            W(
                "_msgPackOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);");
            W("}");

            W($"public static void Serialize(IBufferWriter<byte> buffer, {n} model)");
            W("{");
            W("InitMsgPackOptions();");
            W("MessagePackSerializer.Serialize(buffer, model, _msgPackOptions);");
            W("}");

            W($"public static {n} Deserialize(in ReadOnlyMemory<byte> buffer)");
            W("{");
            W("InitMsgPackOptions();");
            W($"return MessagePackSerializer.Deserialize<{n}>(buffer, _msgPackOptions);");
            W("}");
        }

        private void WriteNotifyPropertyChangedMembers(ClassDeclInfo typeInfo, StreamWriter target)
        {
            target.WriteLine("public event PropertyChangedEventHandler PropertyChanged;");

            target.WriteLine("[NotifyPropertyChangedInvocator]");

            target.WriteLine(typeInfo.IsStruct
                ? " private void OnPropertyChanged([CallerMemberName] string propertyName = null)"
                : " protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)"
            );

            target.WriteLine("{");
            target.WriteLine("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
            target.WriteLine("}");
        }


        private void WriteUsings(ClassDeclInfo typeInfo, StreamWriter csharpWriter, bool notifyPropertyChanged)
        {
            var usings = new List<string>
            {
                "using System;",
                "using System.Collections.Generic;"
            };

            if (_options.JetbrainsAnnotation)
                usings.Add("using JetBrains.Annotations;");

            if (_options.Json)
                usings.Add("using Newtonsoft.Json;");

            if (_options.Unity)
            {
                usings.Add("using UnityEngine;");
                usings.Add("using UnityEngine.Serialization;");
            }

            if (notifyPropertyChanged)
            {
                usings.Add("using System.ComponentModel;");
                usings.Add("using System.Runtime.CompilerServices;");
            }

            if (_options.MsgPack)
            {
                usings.Add("using MessagePack;");
                usings.Add("using System.Buffers;");
            }

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

        private void WriteClassDecl(ClassDeclInfo typeDeclInfo, StreamWriter csharpWriter, bool notifyPropertyChanged)
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

            if (_options.MsgPack)
            {
                csharpWriter.WriteLine("[MessagePackObject]");
            }

            csharpWriter.WriteLine("[System.Serializable]");
            csharpWriter.Write("public partial");
            csharpWriter.Write(typeDeclInfo.IsStruct ? " struct " : " class ");
            csharpWriter.Write(typeDeclInfo.Name);

            if (notifyPropertyChanged)
            {
                csharpWriter.Write(" : INotifyPropertyChanged");
            }

            csharpWriter.WriteLine("{");
        }

        private void WriteFields(ClassDeclInfo typeInfo, StreamWriter cs, bool notifyPropertyChanged)
        {
            void W(string text)
            {
                cs.Write(text);
            }

            void Wln(string line)
            {
                cs.WriteLine(line);
            }

            for (var i = 0; i < typeInfo.Fields.Count; i++)
            {
                var field = typeInfo.Fields[i];
                string fieldType = field.Type.Replace("?", "");

                string typeResult = TypeMap.GetTypeName(fieldType);

                if (_options.Unity)
                    Wln(field.IsNullable ? "[SerializeReference]" : "[SerializeField]");

                string csharpFieldName = GetFieldName(field);

                if (_options.Json)
                {
                    string jsonName = null;
                    foreach (string attribute in field.Attributes)
                    {
                        Match m = JsonNameRegex.Match(attribute);
                        if (m.Success)
                        {
                            string value = m.Groups["name"].Value;
                            jsonName = value.Trim('"');
                        }
                    }

                    if (jsonName == null)
                        jsonName = field.Name.FirstCharToUpper();

                    Wln($"[JsonProperty(\"{jsonName}\")]");
                }

                if (_options.MsgPack)
                {
                    Wln($"[Key({i})]");
                }

                if (_options.Unity)
                {
                    W("[Tooltip(\"");
                    W(string.Join(' ', field.Docs).Trim());
                    Wln("\")]");
                }

                foreach (string attribute in field.Attributes)
                {
                    if (_options.Unity)
                    {
                        Match m = FormerlySerializedRegex.Match(attribute);
                        if (m.Success)
                        {
                            string value = m.Groups["name"].Value;
                            if (value != csharpFieldName)
                            {
                                Wln($"[FormerlySerializedAs(\"{value}\")]");
                            }
                        }
                    }
                }

                Wln($"private {typeResult} {csharpFieldName};");

                Wln("/// <summary>");
                foreach (string doc in field.Docs)
                {
                    W("/// ");
                    Wln(doc);
                }

                Wln("/// </summary>");

                if (_options.JetbrainsAnnotation && field.IsNullable)
                    Wln("[CanBeNull]");

                if (_options.Json)
                    Wln("[JsonIgnore]");

                Wln($"public {typeResult} {field.Name.FirstCharToUpper()}");
                Wln("{");
                Wln($" get => this.{csharpFieldName};");
                if (notifyPropertyChanged)
                {
                    Wln("set");
                    Wln("{");
                    Wln($"if (value == this.{csharpFieldName}) return;");
                    Wln($"this.{csharpFieldName} = value;");
                    Wln("OnPropertyChanged();");
                    Wln("}");
                }
                else
                {
                    Wln($"set => this.{csharpFieldName} = value;");
                }

                Wln("}");

                Wln("");
            }
        }

        private static string GetFieldName(FieldDeclInfo field)
        {
            return "@" + field.Name;
        }

        private static void WriteConstructor(ClassDeclInfo typeInfo, StreamWriter cs)
        {
            if (typeInfo.IsStruct == false)
            {
                cs.WriteLine($"public {typeInfo.Name}() {{}}");
            }

            List<FieldDeclInfo> list = typeInfo.Fields;
            if (list.Count == 0)
                return;

            cs.Write("public " + typeInfo.Name + "(");
            for (int i = 0; i < list.Count; i++)
            {
                FieldDeclInfo field = list[i];
                string typeResult = TypeMap.GetTypeName(field.Type.Replace("?", ""));
                cs.Write(typeResult);
                cs.Write(" @");
                cs.Write(field.Name);
                if (i < list.Count - 1)
                    cs.Write(", ");
            }

            cs.WriteLine(")");
            cs.WriteLine("{");
            foreach (FieldDeclInfo field in list)
            {
                cs.WriteLine("this.@" + field.Name + " = @" + field.Name + ";");
            }

            cs.WriteLine("}");
        }
    }
}