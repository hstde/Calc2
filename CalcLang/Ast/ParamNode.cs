using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class ParamNode : AstNode
    {
        public IdentifierNode Ident;
        public bool IsParams;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var mapped = parseNode.GetMappedChildNodes();

            if (mapped.Count > 1)
            {
                Ident = (IdentifierNode)AddChild("Ident", mapped[1]);
                IsParams = true;
            }
            else
                Ident = (IdentifierNode)AddChild("Ident", mapped[0]);
        }
    }
}
