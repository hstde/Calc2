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
    public class CoalescenceNode : AstNode
    {
        private AstNode target;
        private AstNode alternative;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var nodes = parseNode.GetMappedChildNodes();

            target = AddChild("target", nodes[0]);
            alternative = AddChild("target", nodes[1]);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            var ret = target.Evaluate(thread);

            if (ret == null || thread.Runtime.NullValue.Equals(ret))
                ret = alternative.Evaluate(thread);

            thread.CurrentNode = this.Parent;
            return ret;
        }
    }
}
