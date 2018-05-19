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
        private bool isExtension;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            target = AddChild("Target", nodes[1]) as IdentifierNode;
            parameters = AddChild("Parameters", nodes[2]) as ParamListNode;
            isExtension = string.Equals(nodes[3].FindTokenAndGetText(), "extension", StringComparison.CurrentCultureIgnoreCase);

            AsString = "<extern " + (isExtension ? "Extension " : "") + "Function " + target.AsString + "[" + parameters.ChildNodes.Count + "]>";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            if (isExtension)
                EvaluateExtension(thread);
            else
                EvaluateNormal(thread);

            thread.CurrentNode = this.Parent;
            return thread.Runtime.NullValue;
        }

        private void EvaluateExtension(ScriptThread thread)
        {
            var ident = target.Symbol;

            IBindingSource cons;
            MethodTable mTable = null;
            if (thread.Runtime.ExtensionFunctions.TryGetValue(ident, out cons))
            {
                mTable = ((cons as BuiltInCallableTargetInfo).BindingInstance as ConstantBinding).Target as MethodTable;
            }
            else
            {
                mTable = new MethodTable(ident);
                var targetInfo = new BuiltInCallableTargetInfo(mTable);
                thread.Runtime.ExtensionFunctions.Add(ident, targetInfo);
            }

            var binding = thread.Runtime.BuiltIns.Bind(new BindingRequest(thread, this, ident, TypeInfo.Function, BindingRequestFlags.Existing | BindingRequestFlags.Extern | BindingRequestFlags.Read));

            if (binding == null)
                thread.ThrowScriptError("Extern Symbol {0} not found.", ident);
            var obj = binding.GetValueRef(thread) as MethodTable;

            if (obj == null)
                thread.ThrowScriptError("Extern symbol {0} was not a method table!", ident);

            var tar = obj.GetIndex(parameters.ParamTypes) as BuiltInCallTarget;

            if (tar == null)
                thread.ThrowScriptError("Extern symbol {0} with {1} parameters was not found.", target.Symbol, parameters.ChildNodes.Count);

            tar.ParamCount = parameters.ChildNodes.Count;
            tar.ParamNames = parameters.ParamNames;
            tar.HasParamsArg = parameters.HasParamsArg;
            tar.ParamTypes = parameters.ParamTypes;

            mTable.Add(tar);
        }

        private void EvaluateNormal(ScriptThread thread)
        {
            var mTable = target.Evaluate(thread) as MethodTable;
            bool createNew = mTable == null;

            var binding = thread.Runtime.BuiltIns.Bind(new BindingRequest(thread, this, target.Symbol, TypeInfo.NotDefined, BindingRequestFlags.Existing | BindingRequestFlags.Extern | BindingRequestFlags.Read));


            if (binding == null)
                thread.ThrowScriptError("Extern Symbol {0} not found.", target.Symbol);

            var obj = binding.GetValueRef(thread) as MethodTable;

            if (obj == null)
                thread.ThrowScriptError("Extern symbol {0} was not a method table!", target.Symbol);

            var tar = obj.GetIndex(parameters.ParamTypes) as BuiltInCallTarget;

            if (tar == null)
                thread.ThrowScriptError("Extern symbol {0} with {1} parameters was not found.", target.Symbol, parameters.ChildNodes.Count);

            tar.ParamCount = parameters.ChildNodes.Count;
            tar.ParamNames = parameters.ParamNames;
            tar.HasParamsArg = parameters.HasParamsArg;
            tar.ParamTypes = parameters.ParamTypes;

            if (createNew)
            {
                mTable = new MethodTable(target.Symbol);
                mTable.Add(tar);
                target.DoCreate(thread, mTable, TypeInfo.Function);
            }
            else
            {
                if (mTable.GetIndex(parameters.ParamTypes) != null)
                    thread.ThrowScriptError("Function {0}({1}) is already defined!", target.Symbol, string.Join(", ", parameters.ParamNames));
                mTable.Add(tar);
                target.DoSetValue(thread, mTable, TypeInfo.Function);
            }
        }
    }
}
