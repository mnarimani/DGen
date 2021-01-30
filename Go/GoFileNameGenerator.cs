using System.IO;
using DGen.Common;

namespace Go
{
    public class GoFileNameGenerator : IFileNameGenerator
    {
        private readonly string _baseDirectory;

        public GoFileNameGenerator(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public string GetFilePath(string subPath, string className)
        {
            string fileName = className.Replace("_", "").ToLower() + ".go";

            return Path.Combine(_baseDirectory, subPath.ToLower(), fileName);
        }
    }
}