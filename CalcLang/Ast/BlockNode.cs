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
    public class BlockNode : AstNode
    {

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();

            foreach (var child in nodes)
                if (child.AstNode != null)
                    AddChild("", child);

            AsString = "Block";
            if (ChildNodes.Count == 0)
                AsString += " (Empty)";
        }

        public override void Reset()
        {
            DependentScopeInfo = null;
            base.Reset();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            if (DependentScopeInfo == null)
                DependentScopeInfo = new ScopeInfo(this, Parent?.AsString ?? "");
            FlowControl = FlowControl.Next;
            var lastFlowControl = FlowControl.Next;

            object result = thread.Runtime.NullValue;

            thread.PushScope(DependentScopeInfo, null);

            for (int i = 0; i < ChildNodes.Count && lastFlowControl == FlowControl.Next; i++)
            {
                result = ChildNodes[i].Evaluate(thread);
                lastFlowControl = ChildNodes[i].FlowControl;
            }

            thread.PopScope();

            if (lastFlowControl != FlowControl.Next)
                FlowControl = lastFlowControl;

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
