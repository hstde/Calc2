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
    public class StatementListNode : AstNode
    {
        private AstNode singleChild;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();

            foreach (var child in nodes)
            {
                if (child.AstNode != null)
                    AddChild(string.Empty, child);
            }
            AsString = "Statement List";
            if (ChildNodes.Count == 0)
                AsString += " (Empty)";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            lock (lockObject)
            {
                switch (ChildNodes.Count)
                {
                    case 0:
                        Evaluate = EvaluateEmpty;
                        break;
                    case 1:
                        singleChild = ChildNodes[0];
                        Evaluate = EvaluteOne;
                        break;
                    default:
                        Evaluate = EvaluateMultiple;
                        break;
                }
            }
            var result = Evaluate(thread);
            thread.CurrentNode = Parent;
            return result;
        }

        private object EvaluateMultiple(ScriptThread thread)
        {
            thread.CurrentNode = this;
            FlowControl = FlowControl.Next;

            object result = thread.Runtime.NullValue;

            for (int i = 0; i < ChildNodes.Count && FlowControl == FlowControl.Next; i++)
                result = ChildNodes[i].Evaluate(thread);

            if (FlowControl != FlowControl.Next && Parent != null)
                Parent.FlowControl = FlowControl;

            thread.CurrentNode = Parent;
            return result;
        }

        private object EvaluateEmpty(ScriptThread thread) => thread.Runtime.NullValue;

        private object EvaluteOne(ScriptThread thread)
        {
            thread.CurrentNode = this;
            FlowControl = FlowControl.Next;

            object result = singleChild.Evaluate(thread);

            if (FlowControl != FlowControl.Next && Parent != null)
                Parent.FlowControl = FlowControl;

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
