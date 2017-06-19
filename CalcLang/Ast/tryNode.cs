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
    public class TryNode : AstNode
    {
        private AstNode tryBlock;
        private CatchNode catchBlock;
        private AstNode finallyBlock;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();


        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            try
            {
                tryBlock.Evaluate(thread);
            }
            catch(ScriptException e)
            {
                catchBlock.Evaluate(thread);
            }
            catch(Exception e)
            {
                var se = new ScriptException("UnexpectedExceptionException", e);
                catchBlock.Evaluate(thread);
            }
            finally
            {
                finallyBlock.Evaluate(thread);
            }

            thread.CurrentNode = this.Parent;
            return thread.Runtime.NullValue;
        }
    }
}
