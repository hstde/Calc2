using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class LiteralValueNode : AstNode
    {
        public object Value;
        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            Value = parseNode.Token.Value;
            AsString = Value == null ? "null" : Value.ToString();
            if (Value is string)
                AsString = "\"" + AsString + "\"";
        }

        protected override object DoEvaluate(ScriptThread thread) => Value;
        public override bool IsConstant() => true;
    }
}
