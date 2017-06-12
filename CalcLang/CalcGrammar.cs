using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using CalcLang.Interpreter;
using CalcLang.Ast;

namespace CalcLang
{
    [Language("CalcLang", "0.7", "A simple language inspired by C# and Lua")]
    public class CalcGrammar : Grammar, ICanRunSample
    {
        public CalcGrammar() : base(true)
        {
            var comment = new CommentTerminal("comment", "//", "\n", "\r");
            var blockComment = new CommentTerminal("blockComment", "/*", "*/");
            NonGrammarTerminals.Add(comment);
            NonGrammarTerminals.Add(blockComment);

            NonTerminal start = new NonTerminal("start", typeof(StatementListNode));
            NonTerminal block = new NonTerminal("block");
            NonTerminal instructions = new NonTerminal("instructions", typeof(BlockNode));
            NonTerminal instruction = new NonTerminal("instruction");
            NonTerminal embeddedInstruction = new NonTerminal("embeddedInstruction");
            NonTerminal ifClause = new NonTerminal("ifClause", typeof(IfNode));
            NonTerminal ifElseClause = new NonTerminal("ifElseClause", typeof(IfNode));
            NonTerminal forClause = new NonTerminal("forClause", typeof(ForNode));
            NonTerminal forInitClause = new NonTerminal("forInit", typeof(StatementListNode));
            NonTerminal forConditionClause = new NonTerminal("forCondition");
            NonTerminal forIterClause = new NonTerminal("forIter", typeof(StatementListNode));
            NonTerminal foreachClause = new NonTerminal("foreachClause", typeof(ForeachNode));
            NonTerminal foreachVarDecl = new NonTerminal("foreachVarDecl");
            NonTerminal whileClause = new NonTerminal("whileClause", typeof(WhileNode));
            NonTerminal doWhileClause = new NonTerminal("doWhileClause", typeof(DoWhileNode));
            NonTerminal returnClause = new NonTerminal("returnClause", typeof(ReturnNode));
            NonTerminal emptyReturnClause = new NonTerminal("emptyReturnClause", typeof(ReturnNode));
            NonTerminal breakClause = new NonTerminal("breakClause", typeof(BreakNode));
            NonTerminal continueClause = new NonTerminal("continueClause", typeof(ContinueNode));
            NonTerminal usingClause = new NonTerminal("usingClause", typeof(UsingNode));
            NonTerminal assignment = new NonTerminal("assignment", typeof(AssignmentNode));
            NonTerminal assignmentOp = new NonTerminal("assignmentOp", "assignment operator");
            NonTerminal varDeclaration = new NonTerminal("varDeclaration", typeof(VarDeclarationNode));
            NonTerminal varDeclarationAndAssign = new NonTerminal("varDeclaration", typeof(VarDeclarationNode));
            NonTerminal functionDef = new NonTerminal("functionDef", typeof(FunctionDefNode));
            NonTerminal functionBody = new NonTerminal("functionBody", typeof(StatementListNode));
            NonTerminal inlineFunctionDef = new NonTerminal("inlineFunctionDef", typeof(LambdaNode));
            NonTerminal paramList = new NonTerminal("paramList", typeof(ParamListNode));
            NonTerminal param = new NonTerminal("param", typeof(ParamNode));
            NonTerminal paramsOrEmpty = new NonTerminal("paramsOrEmpty");
            NonTerminal arrayDef = new NonTerminal("arrayDef");
            NonTerminal arrayDefList = new NonTerminal("arrayDefList", typeof(ArrayDefNode));
            NonTerminal arrayDefListItem = new NonTerminal("arrayDefListItem");
            NonTerminal namedArrayItem = new NonTerminal("namedArrayItem", typeof(NamedArrayItemNode));
            NonTerminal expr = new NonTerminal("expr");
            NonTerminal prefixExpr = new NonTerminal("prefixExpr", typeof(IncDecNode));
            NonTerminal postfixExpr = new NonTerminal("postfixExpr", typeof(IncDecNode));
            NonTerminal binExpr = new NonTerminal("binExpr", typeof(BinaryOperationNode));
            NonTerminal unExpr = new NonTerminal("unExpr", typeof(UnaryExpressionNode));
            NonTerminal var = new NonTerminal("var");
            NonTerminal objRef = new NonTerminal("objRef");
            NonTerminal memberAccess = new NonTerminal("memberAccess", typeof(MemberAccessNode));
            NonTerminal ternaryIf = new NonTerminal("ternaryIf", typeof(IfNode));
            NonTerminal functionCall = new NonTerminal("functionCall", typeof(FunctionCallNode));
            NonTerminal varList = new NonTerminal("varList", typeof(ExpressionListNode));
            NonTerminal array = new NonTerminal("array");
            NonTerminal singleDimArray = new NonTerminal("singleDimArray", typeof(IndexedAccessNode));

            IdentifierTerminal name = new IdentifierTerminal("name", IdOptions.IsNotKeyword);
            IdentifierTerminal newName = new IdentifierTerminal("newName", IdOptions.IsNotKeyword);
            NumberLiteral number = new NumberLiteral("number", NumberOptions.AllowUnderscore);

            StringLiteral escapedString = new StringLiteral("string", "\"", StringOptions.AllowsAllEscapes);
            StringLiteral nonEscapedString = new StringLiteral("nonEscapedString", "\"", StringOptions.NoEscapes);
            NonTerminal _string = new NonTerminal("String");

            StringLiteral _char = new StringLiteral("Char", "'", StringOptions.IsChar | StringOptions.AllowsAllEscapes);

            NonTerminal boolVal = new NonTerminal("boolVal", typeof(BoolValNode));
            NonTerminal nullVal = new NonTerminal("nullVal", typeof(NullValueNode));
            NonTerminal thisVal = new NonTerminal("thisVal", typeof(ThisNode));
            NonTerminal binOp = new NonTerminal("binOp", "operator");
            NonTerminal unaryOp = new NonTerminal("unaryOp", "operator");
            NonTerminal incDecOp = new NonTerminal("incDecOp", "operator");

            start.Rule = MakeStarRule(start, instruction);
            block.Rule = "{" + instructions + "}";
            instructions.Rule = MakeStarRule(instructions, instruction);
            instruction.Rule = block
                                | embeddedInstruction + ";"
                                | ifClause
                                | ifElseClause
                                | functionDef
                                | returnClause + ";"
                                | emptyReturnClause
                                | breakClause
                                | continueClause
                                | forClause
                                | foreachClause
                                | whileClause
                                | doWhileClause
                                | usingClause
                                | varDeclaration + ";"
                                | ";";
            instruction.ErrorRule = SyntaxError + ";";
            embeddedInstruction.Rule = functionCall | postfixExpr | prefixExpr | assignment | varDeclarationAndAssign;
            ifElseClause.Rule = ToTerm("if") + "(" + expr + ")" + instruction
                            + PreferShiftHere() + "else" + instruction;
            ifClause.Rule = ToTerm("if") + "(" + expr + ")" + instruction;
            forClause.Rule = ToTerm("for") + "(" + forInitClause + ";" + forConditionClause + ";" + forIterClause + ")" + instruction;
            forInitClause.Rule = MakeStarRule(forInitClause, ToTerm(","), embeddedInstruction);
            forConditionClause.Rule = Empty | expr;
            forIterClause.Rule = MakeStarRule(forIterClause, ToTerm(","), embeddedInstruction);
            foreachClause.Rule = ToTerm("foreach") + "(" + foreachVarDecl + "in" + expr + ")" + instruction;
            foreachVarDecl.Rule = varDeclaration | name;
            whileClause.Rule = ToTerm("while") + "(" + expr + ")" + instruction;
            doWhileClause.Rule = ToTerm("do") + instruction + ToTerm("while") + "(" + expr + ")" + ToTerm(";");
            returnClause.Rule = "return" + expr;
            emptyReturnClause.Rule = ToTerm("return") + ";";
            breakClause.Rule = ToTerm("break") + ";";
            continueClause.Rule = ToTerm("continue") + ";";

            usingClause.Rule = ToTerm("using") + nonEscapedString + ";";

            varDeclaration.Rule = "var" + newName;
            varDeclarationAndAssign.Rule = "var" + newName + "=" + expr;
            assignment.Rule = objRef + assignmentOp + expr;
            assignmentOp.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "|=" | "^=" | "&=" | "<<=" | ">>=";
            objRef.Rule = name | array | memberAccess;
            memberAccess.Rule = var + PreferShiftHere() + "." + name;

            functionDef.Rule = "function" + name + "(" + paramList + ")" + ToTerm("extension").Q() + functionBody;
            functionDef.NodeCaptionTemplate = "function #{0}(...)";
            inlineFunctionDef.Rule = ToTerm("function") + "(" + paramList + ")" + functionBody;
            inlineFunctionDef.NodeCaptionTemplate = "function(...)";
            functionBody.Rule = block | returnClause;

            paramList.Rule = MakeStarRule(paramList, ToTerm(","), param);

            param.Rule = paramsOrEmpty + name;
            paramsOrEmpty.Rule = ToTerm("params") | Empty;

            arrayDef.Rule = ToTerm("{") + arrayDefList + "}";
            arrayDefList.Rule = MakeStarRule(arrayDefList, ToTerm(","), arrayDefListItem);
            arrayDefListItem.Rule = namedArrayItem | expr;
            namedArrayItem.Rule = (name + ReduceHere() | _string) + "=" + expr;

            expr.Rule = prefixExpr | postfixExpr | ternaryIf
                        | var | unExpr | binExpr
                        | inlineFunctionDef
                        | arrayDef
                        | assignment;
            binExpr.Rule = expr + binOp + expr;
            binOp.Rule = ToTerm("&&") | "||" | "&" | "|" | "^"
                        | ToTerm("==") | "<=" | ">=" | "<" | ">" | "!="
                        | ToTerm("+") | "-"
                        | ToTerm("*") | "/" | "%"
                        | ToTerm("<<") | ">>";
            prefixExpr.Rule = incDecOp + objRef + ReduceHere();
            postfixExpr.Rule = objRef + PreferShiftHere() + incDecOp;
            unExpr.Rule = unaryOp + expr + ReduceHere();

            var.Rule = objRef
                        | number
                        | boolVal
                        | nullVal
                        | thisVal
                        | _string
                        | _char
                        | functionCall
                        | ("(" + expr + ")");

            _string.Rule = escapedString | "@" + nonEscapedString;

            ternaryIf.Rule = expr + "?" + expr + ":" + expr;
            functionCall.Rule = var + PreferShiftHere() + "(" + varList + ")";
            functionCall.NodeCaptionTemplate = "call #{0}(...)";
            varList.Rule = MakeStarRule(varList, ToTerm(","), expr);

            array.Rule = singleDimArray;
            singleDimArray.Rule = var + PreferShiftHere() + "[" + expr + "]";

            boolVal.Rule = ToTerm("true") | "false";
            nullVal.Rule = ToTerm("null");
            thisVal.Rule = ToTerm("this");

            unaryOp.Rule = ToTerm("-") | "!" | "~";
            incDecOp.Rule = ToTerm("++") | "--";

            MarkPunctuation("(", ")", "?", ":", "[", "]", ";", "{", "}", ".", ",", "@", "return", "if", "else", "for", "while", "function", "break", "continue", "using", "do", "var", "foreach", "in");
            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            RegisterBracePair("{", "}");

            RegisterOperators(10, "?");
            RegisterOperators(15, "&&", "||", "&", "|", "^");
            RegisterOperators(20, "==", "<", "<=", ">", ">=", "!=");
            RegisterOperators(25, "<<", ">>");
            RegisterOperators(30, "+", "-");
            RegisterOperators(40, "*", "/", "%");
            RegisterOperators(60, "!", "~");
            RegisterOperators(70, "++", "--");
            MarkTransient(var, expr, binOp, unaryOp, block, instruction, embeddedInstruction, _string, objRef, array, arrayDef, assignmentOp, arrayDefListItem, incDecOp, functionBody, foreachVarDecl, paramsOrEmpty);

            AddTermsReportGroup("assignment", "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            AddTermsReportGroup("statement", "if", "while", "for", "return", "break", "continue", "using", "do");
            AddTermsReportGroup("variable declaration", "var");
            AddTermsReportGroup("function declaration", "function");
            AddTermsReportGroup("constant", number, escapedString, nonEscapedString);
            AddTermsReportGroup("constant", "null", "false", "true", "this");
            AddTermsReportGroup("unary operator", "+", "-", "!");
            AddTermsReportGroup("operator", "+", "-", "*", "/", "%", "&", "&&", "|", "||", "^", "?", "==", "<=", "<", ">=", ">", "!=", "<<", ">>");
            AddToNoReportGroup("(", "[", "{", ".", ",", "++", "--");

            MarkReservedWords("if", "else", "return", "function", "while", "for", "null", "false", "true", "this", "break", "continue", "using", "do", "var", "foreach", "in", "params");

            number.DefaultFloatType = TypeCode.Double;
            number.DefaultIntTypes = new TypeCode[] { TypeCode.Int64 };
            number.AddPrefix("0x", NumberOptions.Hex);
            number.AddPrefix("0b", NumberOptions.Binary);

            this.Root = start;

            this.LanguageFlags = LanguageFlags.CreateAst;
        }

        public override void BuildAst(LanguageData language, ParseTree parseTree)
        {
            //return;
            var opHandler = new OperatorHandler();
            Util.Check(!parseTree.HasErrors(), "ParseTree has erros, cannot build AST.");
            var astContext = new AstContext(parseTree.FileName, language, opHandler);
            var astBuilder = new Irony.Ast.AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
        }

        public string RunSample(RunSampleArgs args)
        {
            Evaluator eval = new Evaluator(this);

            eval.ClearOutput();
            eval.Evaluate(args.ParsedSample);
            return eval.GetOutput();
        }

        public Runtime CreateRuntime(LanguageData language) => new Runtime(language);
    }
}
