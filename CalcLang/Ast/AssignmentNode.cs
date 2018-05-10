using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class AssignmentNode : AstNode
    {
        public AstNode Target;
        public string AssignmentOp;
        public bool IsAugmented;
        public ExpressionType BinaryExpressionType;
        public AstNode Expression;
        private OperatorImplementation lastUsed;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            Target = AddChild(AstNodeType.ValueWrite, "To", nodes[0]);

            AssignmentOp = nodes[1].FindTokenAndGetText();
            if (string.IsNullOrEmpty(AssignmentOp))
                AssignmentOp = "=";

            BinaryExpressionType = ExpressionType.Assign;

            Expression = AddChild(AstNodeType.ValueRead, "Expression", nodes[nodes.Count - 1]);
            AsString = AssignmentOp + " (assignment)";
            IsAugmented = AssignmentOp.Length > 1;

            if(IsAugmented)
            {
                var ctxt = (AstContext)context;
                ExpressionType = ctxt.OperationHandler.GetOperatorExpressionType(AssignmentOp);
                BinaryExpressionType = ctxt.OperationHandler.GetBinaryOperatorForAugmented(ExpressionType);
                Target.NodeType = AstNodeType.ValueReadWrite;
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            if (IsAugmented)
                Evaluate = EvaluateAugmented;
            else
                Evaluate = EvaluateSimple;

            var value = Evaluate(thread);
            thread.CurrentNode = Parent;
            return value;
        }

        private object EvaluateAugmented(ScriptThread thread)
        {
            thread.CurrentNode = this;
            var value = Target.Evaluate(thread);
            var exprValue = Expression.Evaluate(thread);

            var result = thread.Runtime.ExecuteBinaryOperator(BinaryExpressionType, value, exprValue, ref lastUsed);

            Target.SetValue(thread, result, Runtime.TypeToTypeInfo(result.GetType()));
            thread.CurrentNode = Parent;
            return result;
        }

        private object EvaluateSimple(ScriptThread thread)
        {
            thread.CurrentNode = this;
            var value = Expression.Evaluate(thread);
            Target.SetValue(thread, value, Runtime.TypeToTypeInfo(value.GetType()));
            thread.CurrentNode = Parent;
            return value;
        }
    }
}
