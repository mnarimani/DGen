using System;
using System.Collections.Generic;
using System.IO;
using CSharp;
using Go;

namespace DGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var classes = Parser.Parse(@"
opt gopackage bs;
opt goimport bullshit;
opt using felan;
opt ktimprot mroebullshti;
opt inherit FelanClass;

// This class does nothing
 class A {
// comment for b
 val b int;
// two line comment
// for c
 val c int;
val d string;
// comment after not having comment
val f int; } class CB { val hello int; var bitch string; }");

            var languages = new List<TargetLanguage>
            {
                new TargetLanguage(new CSharpWriter(), new CSharpFormatter(), new CSharpFileNameGenerator()),
                new TargetLanguage(new GoWriter(), new GoFormatter(), new GoFileNameGenerator())
            };

            foreach (var info in classes)
            {
                foreach (var lang in languages)
                {
                    try
                    {
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, lang.FileNameGenerator.GetFileName(info.Name));
                        using (var stream = new StreamWriter(filePath))
                            lang.Writer.Write(info, stream);
                        lang.Formatter?.Format(filePath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}