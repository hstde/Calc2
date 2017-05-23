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
    public class BoolValNode : AstNode
    {
        public bool Value
        {
            get;
            protected set;
        }

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var valueStr = parseNode.FindToken().Text;

            Value = valueStr.Equals("true");
        }

        protected override object DoEvaluate(ScriptThread thread) => Value;
    }
}
