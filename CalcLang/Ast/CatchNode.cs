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
    public class CatchNode : AstNode
    {
        private string exceptionVarName;
        private AstNode finallyBlock;
        private AstNode Block;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();


        }


        protected override object DoEvaluate(ScriptThread thread) =>
            DoEvalutate(thread, new ScriptException("UnexpectedCallException"));

        public object DoEvalutate(ScriptThread thread, ScriptException se)
        {
            thread.CurrentNode = this;



            thread.CurrentNode = this.Parent;
            return thread.Runtime.NullValue;
        }
    }
}
