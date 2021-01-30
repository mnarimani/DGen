using System.IO;
using DGen.Common;

namespace CSharp
{
    public class CSharpFileNameGenerator : IFileNameGenerator
    {
        private readonly string _baseDirectory;

        public CSharpFileNameGenerator(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }
        
        public string GetFilePath(string subPath, string className)
        {
            return Path.Combine(_baseDirectory, subPath, className + ".g.cs");
        }
    }
}