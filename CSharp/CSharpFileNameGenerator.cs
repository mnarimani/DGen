using DGen.Common;

namespace CSharp
{
    public class CSharpFileNameGenerator : IFileNameGenerator
    {
        public string GetFileName(string className)
        {
            return className + ".cs";
        }
    }
}