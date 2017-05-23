using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using CalcLang.Interpreter;

namespace CalcLang.Ast
{
    public class AstContext : Irony.Ast.AstContext
    {
        public string FileName;
        public readonly OperatorHandler OperationHandler;
        public AstContext(string fileName, LanguageData language, OperatorHandler operationHandler = null) : base(language)
        {
            OperationHandler = operationHandler ?? new OperatorHandler();
            DefaultIdentifierNodeType = typeof(IdentifierNode);
            DefaultLiteralNodeType = typeof(LiteralValueNode);
            DefaultNodeType = typeof(AstNode);
            FileName = fileName;
        }
    }
}
