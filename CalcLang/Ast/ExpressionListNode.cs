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
    public class ExpressionListNode : AstNode
    {
        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            foreach (var child in parseNode.ChildNodes)
                AddChild(AstNodeType.Parameter, "expr", child);

            AsString = "Expression List";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            var values = new object[ChildNodes.Count];
            for (int i = 0; i < values.Length; i++)
                values[i] = ChildNodes[i].Evaluate(thread);
            thread.CurrentNode = Parent;
            return values;
        }
    }
}
