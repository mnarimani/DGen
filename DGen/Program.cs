using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using CSharp;
using DGen.Models;
using Go;

namespace DGen
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<CmdOptions>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> obj)
        {
            foreach (var error in obj)
            {
                Console.WriteLine(error);
            }
        }

        private static void RunOptions(CmdOptions options)
        {
            var languages = GetTargetLanguages(options);

            var directories = new Queue<DirectoryToProcess>();
            directories.Enqueue(new DirectoryToProcess(options.SourceDirectory ?? "", ""));

            while (directories.Count > 0)
            {
                DirectoryToProcess currentDirectory = directories.Dequeue();
                foreach (string subDirectory in Directory.GetDirectories(currentDirectory.Directory))
                {
                    string subPath = Path.Combine(currentDirectory.SubPath,
                        Path.GetFileNameWithoutExtension(subDirectory)
                        ?? throw new NullReferenceException());

                    directories.Enqueue(new DirectoryToProcess(subDirectory, subPath));
                }

                string[] files = Directory.GetFiles(currentDirectory.Directory, "*.dg");

                foreach (string file in files)
                {
                    Console.WriteLine("Processing: " + file);
                    var classes = Parser.Parse(File.ReadAllText(file));
                    foreach (var info in classes)
                    {
                        Console.WriteLine("Creating class: " + info.Name);
                        foreach (var lang in languages)
                        {
                            try
                            {
                                string filePath = lang.FileNameGenerator.GetFilePath(currentDirectory.SubPath, info.Name);

                                Console.WriteLine("Writing To File: " + filePath);
                                
                                string dir = Path.GetDirectoryName(filePath);
                                
                                Directory.CreateDirectory(dir);

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

        private static List<TargetLanguage> GetTargetLanguages(CmdOptions options)
        {
            var languages = new List<TargetLanguage>();

            if (!string.IsNullOrEmpty(options.CSharpOutput))
                languages.Add(new TargetLanguage(new CSharpWriter(options), new CSharpFormatter(), new CSharpFileNameGenerator(options.CSharpOutput)));

            if (!string.IsNullOrEmpty(options.GoOutput))
                languages.Add(new TargetLanguage(new GoWriter(options), new GoFormatter(), new GoFileNameGenerator(options.GoOutput)));
            return languages;
        }
    }
}