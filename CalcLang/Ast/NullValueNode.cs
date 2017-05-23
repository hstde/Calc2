using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;

namespace CalcLang.Ast
{
    public class NullValueNode : AstNode
    {
        protected override object DoEvaluate(ScriptThread thread) => thread.Runtime.NullValue;
    }
}
