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
    public class UsingNamespaceNode : AstNode
    {
        private string target;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            target = GetChildString(parseNode);
            AsString = target;
        }

        private static string GetChildString(ParseTreeNode parent)
        {
            var ret = "";
            if (parent.ChildNodes.Count > 0)
            {
                ret = parent.ChildNodes[0].FindTokenAndGetText();
                if (parent.ChildNodes.Count > 1)
                {
                    ret += "." + GetChildString(parent.ChildNodes[1]);
                }
                return ret;
            }
            return ret;
        }

        protected override object DoEvaluate(ScriptThread thread) =>
            target.Replace(".", "\\");
    }
}
