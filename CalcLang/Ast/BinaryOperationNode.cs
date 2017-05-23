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
    public class BinaryOperationNode : AstNode
    {
        public AstNode Left, Right;
        public string OpSymbol;
        public ExpressionType Op;

        private OperatorImplementation lastUsed;
        private object constValue;
        private bool isConstant;

        public BinaryOperationNode() { }

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            Left = AddChild("Arg", nodes[0]);
            Right = AddChild("Arg", nodes[2]);
            var opToken = nodes[1].FindToken();
            OpSymbol = opToken.Text;

            var ctxt = context as AstContext;
            Op = ctxt.OperationHandler.GetOperatorExpressionType(OpSymbol);

            ErrorAnchor = opToken.Location;
            AsString = Op + " (operator)";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            switch(Op)
            {
                case ExpressionType.AndAlso:
                    Evaluate = EvaluateAndAlso;
                    break;
                case ExpressionType.OrElse:
                    Evaluate = EvaluateOrElse;
                    break;
                default:
                    Evaluate = DefaultEvaluateImpl;
                    break;
            }

            var result = Evaluate(thread);
            if(IsConstant())
            {
                constValue = result;
                AsString = Op + " (operator) Const= " + constValue;
                Evaluate = EvaluateConst;
            }
            thread.CurrentNode = Parent;
            return result;
        }

        private object EvaluateAndAlso(ScriptThread thread)
        {
            var leftValue = Left.Evaluate(thread);
            if (!thread.Runtime.IsTrue(leftValue)) return leftValue;
            return Right.Evaluate(thread);
        }

        private object EvaluateOrElse(ScriptThread thread)
        {
            var leftValue = Left.Evaluate(thread);
            if (thread.Runtime.IsTrue(leftValue)) return leftValue;
            return Right.Evaluate(thread);
        }

        private object DefaultEvaluateImpl(ScriptThread thread)
        {
            thread.CurrentNode = this;
            var arg1 = Left.Evaluate(thread);
            var arg2 = Right.Evaluate(thread);
            var result = thread.Runtime.ExecuteBinaryOperator(Op, arg1, arg2, ref lastUsed);
            thread.CurrentNode = Parent;
            return result;
        }

        private object EvaluateConst(ScriptThread thread) => constValue;

        public override bool IsConstant()
        {
            if (isConstant) return true;
            isConstant = Left.IsConstant() && Right.IsConstant();
            return isConstant;
        }
    }
}
