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
    public class LambdaNode : AstNode
    {
        public ParamListNode Parameters;
        public AstNode Body;
        public IdentifierNode NameNode;

        public LambdaNode()
        {
        }

        public LambdaNode(Irony.Ast.AstContext context, AstNode nameNode, ParseTreeNode node, ParseTreeNode parameters, ParseTreeNode body)
        {
            NameNode = nameNode as IdentifierNode;
            InitImpl(context, node, parameters, body);
        }

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            NameNode = null;
            var nodes = parseNode.GetMappedChildNodes();
            InitImpl(context, parseNode, nodes[0], nodes[1]);
        }

        private void InitImpl(Irony.Ast.AstContext context, ParseTreeNode node, ParseTreeNode parameters, ParseTreeNode body)
        {
            base.Init(context, node);
            lock (lockObject)
            {
                Parameters = AddChild("Parameters", parameters) as ParamListNode;
            }
            Body = AddChild("Body", body);
            AsString = "Lambda[" + Parameters.ChildNodes.Count + "]";
        }

        public override void Reset()
        {
            DependentScopeInfo = null;
            base.Reset();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            lock (lockObject)
            {
                if (DependentScopeInfo == null)
                {
                    DependentScopeInfo = new ScopeInfo(this);
                }

                thread.PushScope(DependentScopeInfo, null);
                Parameters.Evaluate(thread);
                thread.PopScope();
                Evaluate = EvaluateAfter;
            }

            var result = Evaluate(thread);
            thread.CurrentNode = Parent;
            return result;
        }

        private object EvaluateAfter(ScriptThread thread)
        {
            thread.CurrentNode = this;
            var closure = new Closure(thread.CurrentScope, this);
            thread.CurrentNode = Parent;
            return closure;
        }

        public object EvaluateNamed(ScriptThread thread, MethodTable table)
        {
            object res = DoEvaluate(thread);

            var closure = res as Closure;

            table.Add(closure);

            return res;
        }

        public object Call(Scope creatorScope, ScriptThread thread, object thisRef, object[] parameters)
        {
            var save = thread.CurrentNode;
            thread.CurrentNode = this;

            CheckParams(thread, ref thisRef, ref parameters);

            object[] par = new object[] { thisRef }.Concat(parameters).ToArray();

            thread.PushClosureScope(DependentScopeInfo, creatorScope, par);

            lock (lockObject)
            {
                Parameters.Evaluate(thread);
            }
            var result = Body.Evaluate(thread);

            thread.PopScope();
            thread.CurrentNode = save;

            if (!(Body is BlockNode) && !(Body is ReturnNode))
                FlowControl = FlowControl.Return;

            if (FlowControl == FlowControl.Return)
                return result;
            else
                return thread.Runtime.NullValue;
        }

        private void CheckParams(ScriptThread thread, ref object thisRef, ref object[] parameters)
        {
            if (thisRef == null)
                thisRef = thread.Runtime.NullValue;

            if (parameters.Length < Parameters.ChildNodes.Count)
            {
                object[] tmpParas = new object[Parameters.ChildNodes.Count];
                Array.Copy(parameters, tmpParas, parameters.Length);
                for (int i = parameters.Length; i < tmpParas.Length; i++)
                    tmpParas[i] = thread.Runtime.NullValue;

                parameters = tmpParas;
            }
        }
    }
}
