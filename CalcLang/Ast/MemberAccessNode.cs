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
    public class MemberAccessNode : AstNode
    {
        public AstNode Target;
        public object lastTargetValue;
        private string memberName;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            Target = AddChild("Target", nodes[0]);
            memberName = nodes[1].FindTokenAndGetText();
            ErrorAnchor = nodes[1].Span.Location;
            AsString = "." + memberName;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object result = thread.Runtime.NullValue;

            var targetValue = Target.Evaluate(thread);
            lastTargetValue = targetValue;

            var type = targetValue == null || targetValue == thread.Runtime.NullValue ? typeof(NullClass) : targetValue.GetType();
            if (type == typeof(DataTable))
            {
                result = ((DataTable)targetValue).GetString(thread, memberName);
            }

            if (result == thread.Runtime.NullValue)
                result = GetExtension(thread, memberName);
            if (result == thread.Runtime.NullValue)
                if (type == typeof(NullClass))
                    thread.ThrowScriptError("NullReferenceException: \"" + Target.AsString + "\" was null.");
                else
                    thread.ThrowScriptError("MemberAccessException: \"" + memberName + "\" is not a member of \"" + Target.AsString + "\".");

            thread.CurrentNode = Parent;
            return result;
        }

        public override void DoSetValue(ScriptThread thread, object value)
        {
            thread.CurrentNode = this;

            var targetValue = Target.Evaluate(thread);
            lastTargetValue = targetValue;
            if (targetValue == null)
                thread.ThrowScriptError("Target object is null");

            if (targetValue == thread.Runtime.NullValue)
                thread.ThrowScriptError("NullReferenceException. target was null (" + Target.AsString + ")");

            var type = targetValue.GetType();
            if (type == typeof(DataTable))
            {
                ((DataTable)targetValue).SetString(thread, memberName, value);
            }
            else
            {
                thread.ThrowScriptError("MemberAccessException");
            }

            thread.CurrentNode = Parent;
        }

        private object GetBuiltIn(ScriptThread thread, string member)
        {
            var bind = thread.Runtime.BuiltIns.Bind(new BindingRequest(thread, this, member, BindingRequestFlags.Read));

            if (bind != null)
                return bind.GetValueRef(thread);

            return thread.Runtime.NullValue;
        }

        private object GetExtension(ScriptThread thread, string member)
        {
            var bind = thread.Runtime.ExtensionFunctions.Bind(new BindingRequest(thread, this, member, BindingRequestFlags.Read));

            if (bind != null)
                return bind.GetValueRef(thread);

            return thread.Runtime.NullValue;
        }
    }
}
