using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Ast;

namespace CalcLang.Interpreter
{
    public class AppDataMap
    {
        private AstNode programRoot;
        public AstNode ProgramRoot
        {
            get{ return programRoot; }
            set
            {
                programRoot = value;
                var mainScope = StaticScopeInfos.FirstOrDefault();
                if (mainScope != null)
                    mainScope.OwnerNode = value;
            }
        }
        public ScopeInfoList StaticScopeInfos = new ScopeInfoList();

        public ModuleInfoList Modules = new ModuleInfoList();
        public ModuleInfo MainModule;

        public AppDataMap(AstNode programRoot = null)
        {
            ProgramRoot = programRoot ?? new AstNode();
            var mainScopeInfo = new ScopeInfo(ProgramRoot, "Global");
            StaticScopeInfos.Add(mainScopeInfo);
            mainScopeInfo.StaticIndex = 0;
            MainModule = new ModuleInfo("main", "main", mainScopeInfo);
            Modules.Add(MainModule);
        }

        public ModuleInfo GetModule(AstNode moduleNode)
        {
            foreach (var m in Modules)
                if (m.ScopeInfo == moduleNode.DependentScopeInfo)
                    return m;
            return null;
        }
    }
}
