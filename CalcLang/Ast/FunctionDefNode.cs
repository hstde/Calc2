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
    public class FunctionDefNode : AstNode
    {
        public IdentifierNode NameNode;
        public LambdaNode Lambda;
        public bool IsExtension;
        public bool HasParamsArg;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var nodes = parseNode.GetMappedChildNodes();
            NameNode = AddChild("Name", nodes[0]) as IdentifierNode;
            IsExtension = nodes[2].FindTokenAndGetText() == "extension";
            Lambda = new LambdaNode(context, NameNode, parseNode, nodes[1], nodes[3]) {Parent = this};
            ChildNodes.Add(Lambda);
            AsString = "<" + (IsExtension ? " Extension " : "") + "Function " + NameNode.AsString + "[" + Lambda.Parameters.ChildNodes.Count + "]" + ">";
            HasParamsArg = Lambda.Parameters.HasParamsArg;
            parseNode.AstNode = this;
        }

        public override void Reset()
        {
            DependentScopeInfo = null;
            Lambda.Reset();
            base.Reset();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object res = thread.Runtime.NullValue;
            res = IsExtension ? EvaluateExtension(thread) : EvaluateNormal(thread);

            thread.CurrentNode = Parent;
            return res;
        }

        private object EvaluateExtension(ScriptThread thread)
        {
            var ident = NameNode.Symbol;

            IBindingSource cons;
            MethodTable mTable = null;
            if(thread.Runtime.ExtensionFunctions.TryGetValue(ident, out cons))
            {
                mTable = ((cons as BuiltInCallableTargetInfo).BindingInstance as ConstantBinding).Target as MethodTable;
            }
            else
            {
                mTable = new MethodTable(ident);
                BuiltInCallableTargetInfo targetInfo = new BuiltInCallableTargetInfo(mTable);
                thread.Runtime.ExtensionFunctions.Add(ident, targetInfo);
            }

            var closure = Lambda.EvaluateNamed(thread, mTable);

            return closure;
        }

        private object EvaluateNormal(ScriptThread thread)
        {
            MethodTable mTable = NameNode.Evaluate(thread) as MethodTable;
            bool createNew = mTable == null;

            if (mTable == null)
                mTable = new MethodTable((NameNode as IdentifierNode).Symbol);

            var closure = Lambda.EvaluateNamed(thread, mTable);

            if (createNew)
                NameNode.DoCreate(thread, mTable);
            else
                NameNode.DoSetValue(thread, mTable);
            return closure;
        }
    }
}
