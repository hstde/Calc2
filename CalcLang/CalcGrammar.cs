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
    [Language("CalcLang", "0.15", "A simple language inspired by C# and Lua")]
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
            NonTerminal usingNamespace = new NonTerminal("namespace", typeof(UsingNamespaceNode));
            NonTerminal tryClause = new NonTerminal("tryClause", typeof(TryNode));
            NonTerminal catchClause = new NonTerminal("catchClause", typeof(CatchNode));
            NonTerminal finallyClause = new NonTerminal("finallyClause");
            NonTerminal throwClause = new NonTerminal("throwClause", typeof(ThrowNode));
            NonTerminal assignment = new NonTerminal("assignment", typeof(AssignmentNode));
            NonTerminal assignmentOp = new NonTerminal("assignmentOp", "assignment operator");
            NonTerminal varDeclaration = new NonTerminal("varDeclaration", typeof(VarDeclarationNode));
            NonTerminal varDeclarationAndAssign = new NonTerminal("varDeclaration", typeof(VarDeclarationNode));
            NonTerminal functionDef = new NonTerminal("functionDef", typeof(FunctionDefNode));
            NonTerminal functionBody = new NonTerminal("functionBody", typeof(StatementListNode));
            NonTerminal lambdaBody = new NonTerminal("lambdaBody", typeof(StatementListNode));
            NonTerminal inlineFunctionDef = new NonTerminal("inlineFunctionDef", typeof(LambdaNode));
            NonTerminal externFunctionDef = new NonTerminal("externFunctionDef", typeof(ExternFunctionNode));
            NonTerminal paramList = new NonTerminal("paramList", typeof(ParamListNode));
            NonTerminal param = new NonTerminal("param", typeof(ParamNode));
            NonTerminal lambdaParamList = new NonTerminal("lambdaParamList", typeof(ParamListNode));
            NonTerminal singleLambdaParamList = new NonTerminal("lambdaParamList", typeof(ParamListNode));
            NonTerminal lambdaParam = new NonTerminal("lambdaParam", typeof(ParamNode));
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
            NonTerminal coalescence = new NonTerminal("coalescence", typeof(CoalescenceNode));
            NonTerminal functionCall = new NonTerminal("functionCall", typeof(FunctionCallNode));
            NonTerminal varList = new NonTerminal("varList", typeof(ExpressionListNode));
            NonTerminal array = new NonTerminal("array");
            NonTerminal singleDimArray = new NonTerminal("singleDimArray", typeof(IndexedAccessNode));
            NonTerminal rangeArrayDef = new NonTerminal("rangeArrayDef", typeof(RangeArrayDefNode));
            NonTerminal rangeInclusiveArrayDef = new NonTerminal("rangeInclusiveArrayDef", typeof(RangeArrayDefNode));
            NonTerminal typeInfo = new NonTerminal("typeInfo");
            NonTerminal typeInfoOrEmpty = new NonTerminal("typeInfoOrEmpty");

            IdentifierTerminal name = new IdentifierTerminal("name", IdOptions.IsNotKeyword);
            IdentifierTerminal newName = new IdentifierTerminal("newName", IdOptions.IsNotKeyword);
            NumberLiteral number = new NumberLiteral("number", NumberOptions.AllowUnderscore);

            StringLiteral _string = new StringLiteral("string", "\"", StringOptions.AllowsAllEscapes);

            StringLiteral _char = new StringLiteral("Char", "'", StringOptions.IsChar | StringOptions.AllowsAllEscapes);

            NonTerminal boolVal = new NonTerminal("boolVal", typeof(BoolValNode));
            NonTerminal nullVal = new NonTerminal("nullVal", typeof(NullValueNode));
            NonTerminal thisVal = new NonTerminal("thisVal", typeof(ThisNode));
            NonTerminal binOp = new NonTerminal("binOp", "operator");
            NonTerminal unaryOp = new NonTerminal("unaryOp", "operator");
            NonTerminal incDecOp = new NonTerminal("incDecOp", "operator");

            NonTerminal emptyInstruction = new NonTerminal("emptyInstruction", typeof(EmptyNode));

            start.Rule = MakeStarRule(start, instruction);
            block.Rule = "{" + instructions + "}";
            instructions.Rule = MakeStarRule(instructions, instruction);
            instruction.Rule = block
                                | embeddedInstruction + ";"
                                | ifClause
                                | ifElseClause
                                | functionDef
                                | externFunctionDef + ";"
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
                                | emptyInstruction
                                | tryClause
                                | throwClause + ";";
            emptyInstruction.Rule = ToTerm(";");
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

            tryClause.Rule = "try" + block + (catchClause + finallyClause | finallyClause | catchClause);

            catchClause.Rule = "catch" + ("(" + name + ")").Q() + block;
            finallyClause.Rule = "finally" + block;

            throwClause.Rule = "throw" + expr;

            usingClause.Rule = ToTerm("using") + usingNamespace + ";";

            usingNamespace.Rule = (name + "." + usingNamespace) | name;

            varDeclaration.Rule = "var" + name + typeInfoOrEmpty;
            varDeclarationAndAssign.Rule = "var" + name + typeInfoOrEmpty + "=" + expr;

            assignment.Rule = objRef + assignmentOp + expr;
            assignmentOp.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "|=" | "^=" | "&=" | "<<=" | ">>=" | "**=";
            objRef.Rule = name | array | memberAccess;
            memberAccess.Rule = var + PreferShiftHere() + "." + name;

            functionDef.Rule = "function" + name + "(" + paramList + ")" + ToTerm("extension").Q() + functionBody;
            functionDef.NodeCaptionTemplate = "function #{0}(...)";
            inlineFunctionDef.Rule = (ToTerm("function") + "(" + paramList + ")" + functionBody)
                                        | ("(" + lambdaParamList + ")" + ToTerm("=>") + expr)
                                        | (singleLambdaParamList + "=>" + expr);
            externFunctionDef.Rule = ToTerm("extern") + "function" + name + "(" + paramList + ")" + ToTerm("extension").Q();
            inlineFunctionDef.NodeCaptionTemplate = "function(...)";
            functionBody.Rule = block | returnClause;

            paramList.Rule = MakeStarRule(paramList, ToTerm(","), param);
            lambdaParamList.Rule = MakeStarRule(lambdaParamList, ToTerm(","), lambdaParam);
            singleLambdaParamList.Rule = lambdaParam;

            lambdaParam.Rule = name + ReduceIf("=>", "+", "-", "*", "/", "%", "**", "&", "&&", "|", "||", "^", "==", "<=", ">=", "<", ">", "!=", "<<", ">>", ";", "(", "??");
            param.Rule = paramsOrEmpty + name + typeInfoOrEmpty;
            paramsOrEmpty.Rule = ToTerm("params") | Empty;

            arrayDef.Rule = "{" + arrayDefList + "}";
            arrayDefList.Rule = MakeStarRule(arrayDefList, ToTerm(","), arrayDefListItem);
            arrayDefListItem.Rule = namedArrayItem | expr;
            namedArrayItem.Rule = (name + ReduceHere() | _string) + "=" + expr;

            rangeArrayDef.Rule = expr + PreferShiftHere() + ".." + expr + ((PreferShiftHere() + ":" + expr) | Empty);

            rangeInclusiveArrayDef.Rule = expr + PreferShiftHere() + "..." + expr + ((PreferShiftHere() + ":" + expr) | Empty);

            expr.Rule = prefixExpr | postfixExpr | ternaryIf
                        | inlineFunctionDef
                        | var | unExpr | binExpr
                        | arrayDef
                        | rangeArrayDef
                        | rangeInclusiveArrayDef
                        | assignment
                        | coalescence;

            coalescence.Rule = expr + "??" + expr;

            binExpr.Rule = expr + binOp + expr;
            binOp.Rule = ToTerm("&&") | "||" | "&" | "|" | "^"
                        | ToTerm("==") | "<=" | ">=" | "<" | ">" | "!="
                        | ToTerm("+") | "-"
                        | ToTerm("*") | "/" | "%" | "**"
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
                        | functionCall + ReduceHere()
                        | ("(" + expr + ")");

            ternaryIf.Rule = expr + "?" + expr + ":" + expr;
            functionCall.Rule = var + PreferShiftHere() + "(" + varList + ")";
            functionCall.NodeCaptionTemplate = "call #{0}(...)";
            varList.Rule = MakeStarRule(varList, ToTerm(","), expr);

            array.Rule = singleDimArray;
            singleDimArray.Rule = var + PreferShiftHere() + "[" + expr + "]";

            boolVal.Rule = ToTerm("true") | "false";
            nullVal.Rule = ToTerm("null");
            thisVal.Rule = ToTerm("this");

            typeInfoOrEmpty.Rule = ":" + typeInfo | Empty;
            typeInfo.Rule = ToTerm("string")
                            | "function"
                            | "number"
                            | "bool"
                            | "table"
                            | "char";


            unaryOp.Rule = ToTerm("-") | "!" | "~";
            incDecOp.Rule = ToTerm("++") | "--";

            MarkPunctuation("(", ")", "?", ":", "[", "]", ";", "{", "}", ".", ",", "@", "=>", "??",
                "return", "if", "else", "for", "while", "break", "continue",
                "using", "do", "var", "foreach", "in",
                "try", "catch", "finally", "throw", "extern");
            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            RegisterBracePair("{", "}");

            RegisterOperators(10, "?");
            RegisterOperators(15, "&&", "||", "&", "|", "^");
            RegisterOperators(20, "==", "<", "<=", ">", ">=", "!=");
            RegisterOperators(25, "<<", ">>");
            RegisterOperators(30, "+", "-");
            RegisterOperators(40, "*", "/", "%", "**");
            RegisterOperators(60, "!", "~");
            RegisterOperators(70, "++", "--", "??");
            MarkTransient(var, expr, binOp, unaryOp, block, instruction, embeddedInstruction, objRef, array, arrayDef, assignmentOp, arrayDefListItem, incDecOp, functionBody, lambdaBody, foreachVarDecl, paramsOrEmpty, typeInfoOrEmpty);

            AddTermsReportGroup("assignment", "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            AddTermsReportGroup("statement", "if", "while", "for", "return", "break", "continue", "using", "do", "try", "throw", "foreach");
            AddTermsReportGroup("variable declaration", "var");
            AddTermsReportGroup("function declaration", "function", "extern");
            AddTermsReportGroup("constant", number, _string, _char);
            AddTermsReportGroup("constant", "null", "false", "true", "this", "@");
            AddTermsReportGroup("unary operator", "+", "-", "!");
            AddTermsReportGroup("operator", "+", "-", "*", "/", "%", "**", "&", "&&", "|", "||", "^", "?", "==", "<=", "<", ">=", ">", "!=", "<<", ">>", "??", "..");
            AddToNoReportGroup("(", "[", "{", ".", ",", "++", "--");

            MarkReservedWords("if", "else", "return", "function", "while",
                "for", "null", "false", "true", "this", "break", "continue",
                "using", "do", "var", "foreach", "in", "params",
                "try", "catch", "finally", "throw", "extern");

            number.DefaultFloatType = TypeCode.Double;
            number.DefaultIntTypes = new TypeCode[] { TypeCode.Int64 };
            number.AddPrefix("0x", NumberOptions.Hex);
            number.AddPrefix("0b", NumberOptions.Binary);
            number.AddSuffix("d", TypeCode.Double);
            number.AddSuffix("l", TypeCode.Int64);
            number.AddSuffix("m", TypeCode.Decimal);

            _string.AddPrefix("@", StringOptions.NoEscapes);
            _string.AddPrefix("$", StringOptions.IsTemplate | StringOptions.AllowsAllEscapes);

            var stringTemplateSettings = new StringTemplateSettings();
            stringTemplateSettings.StartTag = "{";
            stringTemplateSettings.EndTag = "}";
            stringTemplateSettings.ExpressionRoot = expr;
            this.SnippetRoots.Add(expr);
            _string.AstConfig.NodeType = typeof(StringTemplateNode);
            _string.AstConfig.Data = stringTemplateSettings;

            this.Root = start;

            this.LanguageFlags = LanguageFlags.CreateAst;
        }

        public override void BuildAst(LanguageData language, ParseTree parseTree)
        {
            //return;
            var opHandler = new OperatorHandler();
            Util.Check(!parseTree.HasErrors(), "ParseTree has errors, cannot build AST.");
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
