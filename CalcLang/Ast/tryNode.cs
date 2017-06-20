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

            tryBlock = AddChild("tryBlock", nodes[0]);
            nodes = nodes[1].GetMappedChildNodes();
            if (nodes.Count > 0)
            {
                var isCatch = nodes[0].AstNode is CatchNode;
                if (isCatch)
                    catchBlock = (CatchNode)AddChild("catchBlock", nodes[0]);
                else
                    finallyBlock = AddChild("finallyBlock", nodes[0].GetMappedChildNodes()[0]);
            }
            if(nodes.Count > 1)
            {
                finallyBlock = AddChild("finallyBlock", nodes[1].GetMappedChildNodes()[0]);
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            try
            {
                tryBlock.Evaluate(thread);
            }
            catch (ScriptException e)
            {
                catchBlock?.DoEvalutate(thread, e);
            }
            catch (Exception e)
            {
                var se = new ScriptException("UnexpectedExceptionException", e);
                catchBlock?.DoEvalutate(thread, se);
            }
            finally
            {
                finallyBlock?.Evaluate(thread);
            }

            thread.CurrentNode = this.Parent;
            return thread.Runtime.NullValue;
        }
    }
}
