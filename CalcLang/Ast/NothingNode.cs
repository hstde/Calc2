using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class NothingNode : AstNode
    {
        public NothingNode(BnfTerm term)
        {
            Term = term;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            thread.ThrowScriptError(Irony.Resources.ErrNullNodeEval, Term);
            return null;
        }
    }
}
