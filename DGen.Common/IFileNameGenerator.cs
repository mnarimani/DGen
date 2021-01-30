namespace DGen.Common
{
    public interface IFileNameGenerator
    {
        string GetFilePath(string subPath, string className);
    }
}