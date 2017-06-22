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
    public class ExternFunctionNode : AstNode
    {
        private IdentifierNode target;
        private ParamListNode parameters;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            target = AddChild("Target", nodes[0]) as IdentifierNode;
            parameters = AddChild("Parameters", nodes[1]) as ParamListNode;

            AsString = "<extern Function " + target.AsString + "[" + parameters.ChildNodes.Count + "]>";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            var mTable = target.Evaluate(thread) as MethodTable;
            bool createNew = mTable == null;

            if (createNew)
                mTable = new MethodTable(target.Symbol);

            var binding = thread.Runtime.BuiltIns.Bind(new BindingRequest(thread, this, target.Symbol, BindingRequestFlags.Existing | BindingRequestFlags.Extern));


            if (binding == null)
                thread.ThrowScriptError("Extern Symbol {0} not found.", target.Symbol);

            var obj = binding.GetValueRef(thread) as MethodTable;

            if (obj == null)
                thread.ThrowScriptError("Extern symbol {0} was not a method table!", target.Symbol);

            var tar = obj.GetIndex(parameters.ChildNodes.Count) as BuiltInCallTarget;

            tar.ParamCount = parameters.ChildNodes.Count;
            tar.ParamNames = parameters.ParamNames;
            tar.HasParamsArg = parameters.HasParamsArg;

            if (createNew)
                target.DoCreate(thread, obj);
            else
                target.DoSetValue(thread, obj);

            thread.CurrentNode = this.Parent;
            return thread.Runtime.NullValue;
        }
    }
}
