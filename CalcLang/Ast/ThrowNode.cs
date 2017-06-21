using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class ThrowNode : AstNode
    {
        private AstNode throwable;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            throwable = AddChild("Exception", parseNode.GetMappedChildNodes()[0]);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            object payload = throwable.Evaluate(thread);
            thread.ThrowScriptException(payload);
            return thread.Runtime.NullValue;
        }
    }
}
