using System.Collections.Generic;

namespace DGen.Models
{
    public class FieldDeclInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsNullable { get; set; }
        public List<string> Docs { get; set; }
        public List<string> Attributes { get; } = new List<string>();
    }
}