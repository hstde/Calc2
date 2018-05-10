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
        public TypeInfo Type;
        public bool IsAssignment;
        public bool IsTypeConstrained;
        public AstNode Expression;
        public ExpressionType BinaryExpressionType;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var nodes = parseNode.GetMappedChildNodes();
            Target = AddChild(AstNodeType.ValueWrite, "What", nodes[0]) as IdentifierNode;

            Type = TypeInfo.NotDefined;
            if (nodes.Count > 3)
            {
                //all
                IsTypeConstrained = true;
                IsAssignment = true;

                Type = Runtime.StringToTypeInfo(nodes[1].FindTokenAndGetText());

                BinaryExpressionType = ExpressionType.Assign;
                var ctxt = (AstContext)context;
                ExpressionType = ctxt.OperationHandler.GetOperatorExpressionType("=");
                Expression = AddChild(AstNodeType.ValueRead, "Expression", nodes[3]);
            }
            else if(nodes.Count > 2)
            {
                //assign
                IsAssignment = true;

                BinaryExpressionType = ExpressionType.Assign;
                var ctxt = (AstContext)context;
                ExpressionType = ctxt.OperationHandler.GetOperatorExpressionType("=");
                Expression = AddChild(AstNodeType.ValueRead, "Expression", nodes[2]);
            }

            else if(nodes.Count > 1)
            {
                //typeconstrained
                IsTypeConstrained = true;

                Type = Runtime.StringToTypeInfo(nodes[1].FindTokenAndGetText());
            }

            AsString = "Declare " + Target.AsString + (IsTypeConstrained? " as " + Type: "") + (IsAssignment ? " and assign" : "");
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object value = thread.Runtime.NullValue;

            if (IsAssignment)
                value = Expression.Evaluate(thread);

            if (value == null || thread.Runtime.NullValue.Equals(value))
                value = Runtime.GetDefaultValue(Type);

            Target.DoCreate(thread, value, Type);

            thread.CurrentNode = Parent;
            return value;
        }

        public override void DoSetValue(ScriptThread thread, object value, TypeInfo type = TypeInfo.NotDefined)
        {
            Target.DoSetValue(thread, value, Type);
        }
    }
}
