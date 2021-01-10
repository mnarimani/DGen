using System.IO;
using DGen.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharp
{
    public class CSharpFormatter : ICodeFormatter
    {
        public void Format(string file)
        {
            string csCode = File.ReadAllText(file);
            csCode = CSharpSyntaxTree.ParseText(csCode).GetRoot().NormalizeWhitespace().ToFullString();
            File.WriteAllText(file, csCode);
        }
    }
}