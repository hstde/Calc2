using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class UnaryExpressionNode : AstNode
    {
        public string OpSymbol;
        public AstNode Argument;
        private Interpreter.OperatorImplementation lastUsed;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            OpSymbol = nodes[0].FindTokenAndGetText();
            Argument = AddChild("Arg", nodes[1]);
            AsString = OpSymbol + "(unary op)";
            var interpContext = (AstContext)context;
            ExpressionType = interpContext.OperationHandler.GetUnaryOperatorExpressionType(OpSymbol);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object result = null;
            var arg = Argument.Evaluate(thread);

            if (arg.GetType() == typeof(DataTable))
            {
                var dt = arg as DataTable;
                var callTarget = dt?.GetOperatorCallTarget(ExpressionType);

                result = callTarget?.Call(thread, null, new[] { dt ?? arg });
            }

            if (result == null)
                result = thread.Runtime.ExecuteUnaryOperator(ExpressionType, arg, ref lastUsed);

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
