using DGen.Common;

namespace DGen
{
    public class TargetLanguage
    {
        public ICodeWriter Writer { get; }
        public ICodeFormatter Formatter { get; }
        public IFileNameGenerator FileNameGenerator { get; }

        public TargetLanguage(ICodeWriter writer, ICodeFormatter formatter, IFileNameGenerator fileNameGenerator)
        {
            Writer = writer;
            Formatter = formatter;
            FileNameGenerator = fileNameGenerator;
        }
    }
}