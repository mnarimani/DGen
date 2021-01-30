namespace DGen
{
    class DirectoryToProcess
    {
        public string Directory, SubPath;

        public DirectoryToProcess(string directory, string subPath)
        {
            Directory = directory;
            SubPath = subPath;
        }
    }
}