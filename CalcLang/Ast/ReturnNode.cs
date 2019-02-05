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
    public class ReturnNode : AstNode
    {
        public AstNode Expression;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();

            if (nodes.Any())
            {
                Expression = AddChild("Expression", nodes[0]);
            }

            AsString = "return" + (nodes.Any() ? " Expression" : "");
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            object result = thread.Runtime.NullValue;

            if (Expression != null)
                result = Expression.Evaluate(thread);

            FlowControl = FlowControl.Return;

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
