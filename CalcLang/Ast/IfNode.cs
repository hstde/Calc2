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
    public class IfNode : AstNode
    {
        public AstNode Condition;
        public AstNode IfBlock;
        public AstNode ElseBlock;


        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            Condition = AddChild("Condition", nodes[0]);
            IfBlock = AddChild("IfBlock", nodes[1]);
            if (nodes.Count > 2)
                ElseBlock = AddChild("ElseBlock", nodes[2]);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object result = thread.Runtime.NullValue;

            FlowControl = FlowControl.Next;

            var cond = Condition.Evaluate(thread);
            var isTrue = thread.Runtime.IsTrue(cond);
            if(isTrue)
            {
                if (IfBlock != null)
                    result = IfBlock.Evaluate(thread);
            }
            else
            {
                if (ElseBlock != null)
                    result = ElseBlock.Evaluate(thread);
            }

            if (FlowControl != FlowControl.Next && Parent != null)
                Parent.FlowControl = FlowControl;

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
