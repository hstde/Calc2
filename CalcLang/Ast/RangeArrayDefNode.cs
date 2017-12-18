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
    public class RangeArrayDefNode : AstNode
    {
        public AstNode from;
        public AstNode to;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var nodes = parseNode.GetMappedChildNodes();
            from = AddChild("from", nodes[0]);
            to = AddChild("to", nodes[1]);

            AsString = "Range from " + from + " to " + to;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            var from = (long)this.from.Evaluate(thread);
            var to = (long)this.to.Evaluate(thread);

            var result = new DataTable(Enumerable.Range((int)from, (int)(to - from + 1)), thread);

            thread.CurrentNode = Parent;

            return result;
        }
    }
}
