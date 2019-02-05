using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using CalcLang.Interpreter;

namespace CalcLang.Ast
{
    public class BreakNode : AstNode
    {
        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            AsString = "break";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            FlowControl = FlowControl.Break;

            thread.CurrentNode = Parent;
            return thread.Runtime.NullValue;
        }
    }
}
