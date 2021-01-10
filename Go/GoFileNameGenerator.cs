using DGen.Common;

namespace Go
{
    public class GoFileNameGenerator : IFileNameGenerator
    {
        public string GetFileName(string className)
        {
            return className.Replace("_", "").ToLower() + ".go";
        }
    }
}