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
    public class CatchNode : AstNode
    {
        private IdentifierNode exceptionVar;
        private AstNode block;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            var hasVar = nodes[0].ChildNodes[0].ChildNodes.Count > 0;
            if (hasVar)
                exceptionVar = (IdentifierNode)AddChild("exceptionVar", nodes[0].ChildNodes[0].ChildNodes[0]);
            block = AddChild("Block", nodes[1]);

        }


        protected override object DoEvaluate(ScriptThread thread) =>
            DoEvalutate(thread, new ScriptException("UnexpectedCallException"));

        public object DoEvalutate(ScriptThread thread, ScriptException se)
        {
            thread.CurrentNode = this;

            if (DependentScopeInfo == null)
                DependentScopeInfo = new ScopeInfo(this, AsString);

            thread.PushScope(DependentScopeInfo, null);

            var bind = thread.Bind(exceptionVar.Symbol, BindingRequestFlags.NewOnly | BindingRequestFlags.Write);
            if (bind != null && !(bind is NullBinding) && bind.SetValueRef != null)
                bind.SetValueRef(thread, se.ToScriptObject());

            block.Evaluate(thread);

            thread.CurrentNode = this.Parent;
            return thread.Runtime.NullValue;
        }
    }
}
