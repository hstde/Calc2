﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using CalcLang.Interpreter;

namespace CalcLang
{
    public class Evaluator
    {
        public CalcGrammar Grammar { get; }
        public Parser Parser { get; }
        public LanguageData Language { get; }
        public Runtime Runtime { get; private set; }
        public ScriptApp App { get; private set; }

        public IDictionary<string, object> Globals => App.Globals;

        public Evaluator() : this(new CalcGrammar())
        {
        }

        public Evaluator(CalcGrammar grammar)
        {
            Grammar = grammar;
            Language = new LanguageData(grammar);
            Parser = new Parser(Language);
            Runtime = Grammar.CreateRuntime(Language);
            App = new ScriptApp(Runtime);
        }

        public object Evaluate(string script) => App.Evaluate(script);

        public object Evaluate(string script, string fileName, string[] args) => App.Evaluate(script, fileName, args);

        public object Evaluate(ParseTree parsedScript) => App.Evaluate(parsedScript);

        public object Evaluate() => App.Evaluate();

        public void ClearOutput()
        {
            App.ClearOutputBuffer();
        }

        public string GetOutput() => App.GetOutput();

        public void Reset()
        {
            var rethrowBack = App.RethrowExceptions;
            var modeBack = App.ParserMode;
            Runtime = Grammar.CreateRuntime(Language);
            App = new ScriptApp(Runtime);
            App.RethrowExceptions = rethrowBack;
            App.ParserMode = modeBack;
        }
    }
}
