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
    public class IdentifierNode : AstNode
    {
        public string Symbol;
        private Binding binding;

        public IdentifierNode()
        {
        }

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            Symbol = parseNode.Token.ValueString;
            AsString = Symbol;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            if (binding == null || binding is NullBinding)
                binding = thread.Bind(Symbol, BindingRequestFlags.Read | BindingRequestFlags.NullOk);

            Evaluate = binding.GetValueRef;
            var result = Evaluate(thread);

            thread.CurrentNode = Parent;
            return result;
        }

        public override void DoSetValue(ScriptThread thread, object value)
        {
            thread.CurrentNode = this;
            if (binding == null || binding is NullBinding)
                binding = thread.Bind(Symbol, BindingRequestFlags.Write | BindingRequestFlags.ExistingOrNew | BindingRequestFlags.PreferExisting);

            if (binding.SetValueRef == null)
                thread.ThrowScriptError("ups {0} is not writable", Symbol);

            binding.SetValueRef(thread, value);
            thread.CurrentNode = Parent;
        }

        public void DoCreate(ScriptThread thread, object value)
        {
            thread.CurrentNode = this;
            if (binding == null || binding is NullBinding)
                binding = thread.Bind(Symbol, BindingRequestFlags.Write | BindingRequestFlags.NewOnly);

            if (binding.SetValueRef == null)
                thread.ThrowScriptError("could not create {0} for writing", Symbol);

            binding.SetValueRef(thread, value);
            thread.CurrentNode = Parent;
        }
    }
}
