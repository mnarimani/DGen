using System.Security.Cryptography;
using CommandLine;

namespace DGen.Models
{
    public class CmdOptions
    {
        [Option("jetbrains-annotations", Required = false)]
        public bool JetbrainsAnnotation { get; set; }
        
        [Option("json", Required = false)]
        public bool Json { get; set; }

        [Option("json-names", Required = false)]
        public JsonNaming JsonNaming { get; set; } = JsonNaming.PascalCase;
        
        [Option("unity", Required = false)]
        public bool Unity { get; set; }
        
        [Option("msgpack", Required = false)]
        public bool MsgPack { get; set; }
        
        [Option('s', "src", Required = false)]
        public string SourceDirectory { get; set; }
        
        [Option( "csharp-out", Required = false)]
        public string CSharpOutput { get; set; }
        
        [Option( "go-out", Required = false)]
        public string GoOutput { get; set; }
    }
}