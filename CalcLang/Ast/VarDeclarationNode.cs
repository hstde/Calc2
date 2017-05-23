using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class VarDeclarationNode : AstNode
    {
        public IdentifierNode Target;
        public bool IsAssignment;
        public AstNode Expression;
        public ExpressionType BinaryExpressionType;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var nodes = parseNode.GetMappedChildNodes();
            Target = AddChild(AstNodeType.ValueWrite, "What", nodes[0]) as IdentifierNode;

            IsAssignment = nodes.Count > 1;
            if(IsAssignment)
            {
                BinaryExpressionType = ExpressionType.Assign;
                var ctxt = (AstContext)context;
                ExpressionType = ctxt.OperationHandler.GetOperatorExpressionType("=");
                Expression = AddChild(AstNodeType.ValueRead, "Expression", nodes[2]);
            }

            AsString = "Declare " + Target.AsString + (IsAssignment ? " and assign" : "");
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object value = thread.Runtime.NullValue;

            if (IsAssignment)
                value = Expression.Evaluate(thread);

            Target.DoCreate(thread, value);

            thread.CurrentNode = Parent;
            return value;
        }

        public override void DoSetValue(ScriptThread thread, object value)
        {
            Target.DoSetValue(thread, value);
        }
    }
}
