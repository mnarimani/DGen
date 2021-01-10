using System.IO;
using DGen.Models;

namespace DGen.Common
{
    public interface ICodeWriter
    {
        void Write(ClassDeclInfo declInfo, StreamWriter target);
    }
}