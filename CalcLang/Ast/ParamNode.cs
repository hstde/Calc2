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
    public class ParamNode : AstNode
    {
        public IdentifierNode Ident;
        public TypeInfo Type;
        public bool IsTypeConstrained;
        public bool IsParams;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var mapped = parseNode.GetMappedChildNodes();

            if(mapped.Count > 2)
            {
                Ident = (IdentifierNode)AddChild("Ident", mapped[1]);
                Type = Runtime.StringToTypeInfo(mapped[2].FindTokenAndGetText());
                IsParams = true;
                IsTypeConstrained = true;
            }
            else if (mapped.Count > 1)
            {
                //params or typeConstrained
                if (mapped[0].Term.Name == "name")
                {
                    Ident = (IdentifierNode)AddChild("Ident", mapped[0]);
                    Type = Runtime.StringToTypeInfo(mapped[1].FindTokenAndGetText());
                    IsTypeConstrained = true;
                }
                else
                {
                    Ident = (IdentifierNode)AddChild("Ident", mapped[1]);
                    IsParams = true;
                }
            }
            else
                Ident = (IdentifierNode)AddChild("Ident", mapped[0]);
        }
    }
}
