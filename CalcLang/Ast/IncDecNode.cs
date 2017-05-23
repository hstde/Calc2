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
    public class IncDecNode : AstNode
    {
        public bool IsPostfix;
        public string Opsymbol;
        public string BinaryOpSymbol;
        public ExpressionType BinaryOp;
        public AstNode Argument;
        private Interpreter.OperatorImplementation lastUsed;


        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            FindOpAndDetectPostfix(nodes);
            int argIndex = IsPostfix ? 0 : 1;
            Argument = AddChild(AstNodeType.ValueReadWrite, "Arg", nodes[argIndex]);

            BinaryOpSymbol = Opsymbol[0].ToString();
            var interpContext = (AstContext)context;
            BinaryOp = interpContext.OperationHandler.GetOperatorExpressionType(BinaryOpSymbol);
            AsString = Opsymbol + (IsPostfix ? "(postfix)" : "(prefix)");
        }

        private void FindOpAndDetectPostfix(ParseTreeNodeList mappedNodes)
        {
            IsPostfix = false;
            Opsymbol = mappedNodes[0].FindTokenAndGetText();
            if (Opsymbol == "--" || Opsymbol == "++") return;
            IsPostfix = true;
            Opsymbol = mappedNodes[1].FindTokenAndGetText();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            var oldVal = Argument.Evaluate(thread);
            var newVal = thread.Runtime.ExecuteBinaryOperator(BinaryOp, oldVal, 1, ref lastUsed);
            Argument.SetValue(thread, newVal);
            var result = IsPostfix ? oldVal : newVal;
            thread.CurrentNode = Parent;
            return result;
        }
    }
}
