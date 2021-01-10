using System;
using System.Collections.Generic;

namespace DGen.Models
{
    public class ClassDeclInfo
    {
        public string Name { get; set; }
        public bool IsStruct { get; set; }
        public List<string> Docs { get; set; }
        public List<FieldDeclInfo> Fields { get; } = new List<FieldDeclInfo>();
        public List<string> Options { get; } = new List<string>();
    }
}