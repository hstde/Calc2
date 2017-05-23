using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using System.Reflection;

namespace CalcLang.Interpreter
{
    public enum AppStatus
    {
        Ready,
        Evaluating,
        WaitingForMoreInput,
        SyntaxError,
        RuntimeError,
        Crash,
        Aborted
    }

    public class ScriptApp
    {

        private IList<Assembly> ImportedAssemblies = new List<Assembly>();
        private object lockObject = new object();

        public readonly LanguageData Language;
        public readonly Runtime Runtime;
        public AppDataMap DataMap;
        public Scope[] StaticScopes;
        public Scope MainScope;
        public StringBuilder OutputBuffer = new StringBuilder();

        public AppStatus Status;
        public long EvaluationTime;
        public Exception LastException;
        public bool RethrowExceptions = true;

        public ParseTree LastScript
        {
            get;
            private set;
        }
        public Parser Parser
        {
            get;
            private set;
        }
        public IDictionary<string, object> Globals
        {
            get;
            private set;
        }
        public ParseMode ParserMode
        {
            get { return Parser.Context.Mode; }
            set { Parser.Context.Mode = value; }
        }

        public string CurrentFile
        {
            get;
            set;
        }


        public ScriptApp(LanguageData language)
        {
            Language = language;
            var grammar = language.Grammar as CalcGrammar;
            Runtime = grammar.CreateRuntime(language);
            DataMap = new AppDataMap();
            Init();
        }
        public ScriptApp(Runtime runtime)
        {
            Runtime = runtime;
            Language = Runtime.Language;
            DataMap = new AppDataMap();
            Init();
        }
        public ScriptApp(AppDataMap dataMap)
        {
            DataMap = dataMap;
            Init();
        }

        [System.Security.SecuritySafeCritical]
        private void Init()
        {
            Parser = new Parser(Language);
            MainScope = new Scope(DataMap.MainModule.ScopeInfo, null, null, null);
            StaticScopes = new Scope[DataMap.StaticScopeInfos.Count];
            StaticScopes[0] = MainScope;
            Globals = MainScope.AsDictionary();
        }

        public Irony.LogMessageList GetParserMessages() => Parser.Context.CurrentParseTree.ParserMessages;


        public object Evaluate(string script) => Evaluate(script, "<submission>");
        public object Evaluate(string script, string fileName)
        {
            try
            {
                CurrentFile = fileName;
                var parsedScript = Parser.Parse(script, fileName);
                if (parsedScript.HasErrors())
                {
                    Status = AppStatus.SyntaxError;
                    if (RethrowExceptions)
                        throw new ScriptException("Syntax errors found.");
                    return null;
                }

                if (ParserMode == ParseMode.CommandLine && Parser.Context.Status == ParserStatus.AcceptedPartial)
                {
                    Status = AppStatus.WaitingForMoreInput;
                    return null;
                }

                LastScript = parsedScript;
                var result = EvaluateParsedScript();
                return result;
            }
            catch (ScriptException)
            {
                throw;
            }
            catch (Exception e)
            {
                LastException = e;
                Status = AppStatus.Crash;
                return null;
            }
        }

        public object Evaluate(ParseTree parsedScript)
        {
            CurrentFile = "<submission>";
            Util.Check(parsedScript.Root.AstNode != null, "Root AST node is null, cannot evaluate script.");
            var root = parsedScript.Root.AstNode as Ast.AstNode;
            Util.Check(root != null, "Root AST node {0} is not a subclass of AstNode", root.GetType());
            LastScript = parsedScript;
            return EvaluateParsedScript();
        }

        public object Evaluate()
        {
            Util.Check(LastScript != null, "No previously parsed script.");
            return EvaluateParsedScript();
        }

        private object EvaluateParsedScript()
        {
            LastScript.Tag = DataMap;
            var root = LastScript.Root.AstNode as Ast.AstNode;
            root.DependentScopeInfo = MainScope.ScopeInfo;
            DataMap.ProgramRoot = root;

            Status = AppStatus.Evaluating;
            ScriptThread thread = null;
            try
            {
                thread = new ScriptThread(this);

                thread.CurrentFuncInfo = new FunctionInfo("<global>", -1, null);

                var result = root.Evaluate(thread);

                Status = AppStatus.Ready;
                return result;
            }
            catch (ScriptException se)
            {
                Status = AppStatus.RuntimeError;
                se.Location = thread.CurrentNode.Location;
                if (se.ScriptStackTrace == null)
                    se.ScriptStackTrace = thread.GetStackTrace(true);
                LastException = se;
                if (RethrowExceptions)
                    throw;
                return null;
            }
            catch (Exception ex)
            {
                Status = AppStatus.RuntimeError;
                var se = new ScriptException(ex.Message, ex, thread.CurrentNode.Location, thread.GetStackTrace(true));
                LastException = se;
                if (RethrowExceptions)
                    throw se;
                return null;
            }
        }

        public void Write(object obj)
        {
            lock (lockObject)
            {
                OutputBuffer.Append(obj.ToString());
            }
        }

        public void WriteLine(object obj)
        {
            lock (lockObject)
            {
                OutputBuffer.AppendLine(obj.ToString());
            }
        }

        public void ClearOutputBuffer()
        {
            lock (lockObject)
            {
                OutputBuffer.Clear();
            }
        }

        public string GetOutput()
        {
            lock (lockObject)
            {
                return OutputBuffer.ToString().Replace("\r", "").Replace("\n", Environment.NewLine);
            }
        }
    }
}
