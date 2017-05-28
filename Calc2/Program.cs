﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using CalcLang;

namespace Calc2
{
    internal static class Program
    {
        private const string promt = "> ";
        private const string morePromt = ". ";

        private static void Main(string[] args)
        {
            Evaluator eval = new Evaluator();
            string input = "";
            eval.App.RethrowExceptions = false;
            eval.App.ParserMode = ParseMode.CommandLine;

            while (true)
            {
                bool partial = eval.App.Status == CalcLang.Interpreter.AppStatus.WaitingForMoreInput;
                string ppromt = partial ? morePromt : promt;

                Console.Write(ppromt);
                if (partial)
                    input += "\n" + Console.ReadLine();
                else
                    input = Console.ReadLine();

                GC.Collect(0);

                eval.ClearOutput();

                int collections = GC.CollectionCount(0);
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var res = eval.Evaluate(input);
                sw.Stop();
                collections = GC.CollectionCount(0) - collections;

                switch (eval.App.Status)
                {
                    case CalcLang.Interpreter.AppStatus.SyntaxError:
                        Console.WriteLine(eval.GetOutput());
                        foreach (var err in eval.App.GetParserMessages())
                        {
                            Console.WriteLine(string.Empty.PadRight(ppromt.Length + err.Location.Column) + "^");
                            Console.WriteLine(err.Message);
                        }
                        break;
                    case CalcLang.Interpreter.AppStatus.Ready:
                        Console.WriteLine(eval.GetOutput());
                        break;
                    case CalcLang.Interpreter.AppStatus.Crash:
                    case CalcLang.Interpreter.AppStatus.RuntimeError:
                        var ex = eval.App.LastException;
                        var screx = ex as CalcLang.Interpreter.ScriptException;
                        if (screx != null)
                        {
                            Console.WriteLine(screx.ToString());
                            Console.WriteLine(screx.InnerException?.ToString());
                        }
                        else
                        {
                            Console.WriteLine(ex.Message);
                        }
                        break;
                }

#if DEBUG
                if (eval.App.Status != CalcLang.Interpreter.AppStatus.WaitingForMoreInput)
                {
                    Console.WriteLine("[Debug] Evaluation time: " + sw.Elapsed);
                    Console.WriteLine("[Debug] GC Collections: " + collections);
                }
#endif
            }
        }
    }
}