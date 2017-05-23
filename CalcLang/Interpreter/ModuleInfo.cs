using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class ModuleInfoList : List<ModuleInfo> { }
    public class ModuleInfo
    {
        public readonly string Name;
        public readonly string FileName;
        public readonly ScopeInfo ScopeInfo;

        public ModuleInfo(string name, string fileName, ScopeInfo scopeInfo)
        {
            Name = name;
            FileName = fileName;
            ScopeInfo = scopeInfo;
        }
    }
}
