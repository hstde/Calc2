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
        public AstNode step;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var nodes = parseNode.GetMappedChildNodes();
            from = AddChild("from", nodes[0]);
            to = AddChild("to", nodes[1]);
            step = nodes[2].ChildNodes[0].ChildNodes.Count > 0 ? AddChild("step", nodes[2].ChildNodes[0].ChildNodes[0]) : null;

            AsString = "Range from " + from + " to " + to + (step != null ? " step " + step : "");
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            var from = (long)this.from.Evaluate(thread);
            var to = (long)this.to.Evaluate(thread);
            var step = this.step != null ? (long)this.step.Evaluate(thread) : 1;


            DataTable result;
            if (this.step == null)
                result = new DataTable(new Range(from, to), thread);
            else
                result = new DataTable(new RangeWithStep(from, to, step), thread);

            thread.CurrentNode = Parent;

            return result;
        }
    }
}
