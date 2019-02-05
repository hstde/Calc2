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
    public class ForNode : AstNode
    {
        public AstNode InitBlock;
        public AstNode Condition;
        public AstNode IterBlock;
        public AstNode Block;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            InitBlock = AddChild("Init", nodes[0]);
            var t = nodes[1].GetMappedChildNodes();
            Condition = t.Count > 0 ? AddChild("Condition", t[0]) : null;
            IterBlock = AddChild("Iter", nodes[2]);
            Block = AddChild("ForBlock", nodes[3]);

            AsString = "For Loop";
        }

        public override void Reset()
        {
            DependentScopeInfo = null;
            base.Reset();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object result = thread.Runtime.NullValue;
            bool cond = true;

            FlowControl = FlowControl.Next;

            if (DependentScopeInfo == null)
                DependentScopeInfo = new ScopeInfo(this, Parent?.AsString ?? "");

            thread.PushScope(DependentScopeInfo, null);
            InitBlock?.Evaluate(thread);

            if (Condition != null)
                cond = thread.Runtime.IsTrue(Condition.Evaluate(thread));

            while ((Condition == null || cond) && FlowControl == FlowControl.Next)
            {
                result = Block.Evaluate(thread);

                if (Block.FlowControl == FlowControl.Break)
                    break;

                if (Block.FlowControl == FlowControl.Continue)
                    FlowControl = FlowControl.Next;

                if (Block.FlowControl == FlowControl.Return)
                    break;

                IterBlock?.Evaluate(thread);

                if (Condition != null)
                    cond = thread.Runtime.IsTrue(Condition.Evaluate(thread));
            }
            thread.PopScope();

            if (Block.FlowControl == FlowControl.Return)
                FlowControl = FlowControl.Return;

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
