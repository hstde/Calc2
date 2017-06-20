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

            return string.Join(Environment.NewLine, rev.Select(x => "\t@ " + x.ToString()));
        }
    }
}
