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
    public class ThisNode : AstNode
    {
        private Binding binding;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            AsString = "this";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            binding = thread.Bind("this", BindingRequestFlags.Read);
            Evaluate = binding.GetValueRef;
            object result = Evaluate(thread);


            thread.CurrentNode = Parent;
            return result;
        }

        public override void DoSetValue(ScriptThread thread, object value, TypeInfo type = TypeInfo.NotDefined)
        {
            thread.CurrentNode = this;

            thread.ThrowScriptError("this is not writable.");

            //binding = thread.Bind("this", BindingRequestFlags.Write);
            //SetValue = binding.SetValueRef;
            //SetValue(thread, value);

            thread.CurrentNode = Parent;
        }
    }
}