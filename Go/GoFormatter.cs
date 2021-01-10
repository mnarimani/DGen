using DGen.Common;

namespace Go
{
    public class GoFormatter : ICodeFormatter
    {
        public void Format(string file)
        {
            System.Diagnostics.Process.Start("CMD.exe", "/C gofmt -w " + file);
            System.Diagnostics.Process.Start("CMD.exe", "/C goimports -w " + file);
        }
    }
}