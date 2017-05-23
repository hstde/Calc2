using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public struct SourceInfo
    {
        public Irony.Parsing.SourceLocation SourceLocation;
        public string FileName;

        public SourceInfo(Irony.Parsing.SourceLocation loc) : this(loc, "<submission>")
        {
        }

        public SourceInfo(Irony.Parsing.SourceLocation loc, string file)
        {
            FileName = file;
            SourceLocation = loc;
        }

        public static implicit operator SourceInfo(Irony.Parsing.SourceLocation loc) => new SourceInfo(loc);
    }
}
