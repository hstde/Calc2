using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class ScriptStackTrace
    {
        public Stack<FunctionInfo> CallStack;

        public ScriptStackTrace(IEnumerable<FunctionInfo> callStack)
        {
            CallStack = new Stack<FunctionInfo>(callStack);
        }

        public override string ToString()
        {
            var rev = CallStack.Reverse().ToList();
            /*var outList = new List<string>();

            for(int i = 0; i < rev.Count - 1; i++)
            {
                outList.Add(FunctionInfo.MakeString(rev[i + 1].Name, rev[i + 1].MinParamCount, rev[i + 1].ParamNames, rev[i].CallLocation));
            }*/

            return string.Join(Environment.NewLine, rev.Select(x => "\t@ " + x.ToString()));
        }
    }
}
