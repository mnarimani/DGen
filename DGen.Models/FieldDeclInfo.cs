using System.Collections.Generic;

namespace DGen.Models
{
    public class FieldDeclInfo
    {
        public bool IsReadonly;
        public string Name;
        public string Type;
        public List<string> Docs;
        public bool IsNullable { get; set; }
    }
}