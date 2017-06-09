using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;
using System.Reflection;
using System.IO;

namespace CalcLang.Ast
{
    public class UsingNode : AstNode
    {
        private AstNode target;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();

            target = AddChild("Target", nodes[0]);

            AsString = "import Library";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object ret = null;

            string originalPath = target.Evaluate(thread) as string;

            List<Type> delegetateParamsHelper = new List<Type> { typeof(ScriptThread), typeof(object), typeof(object[]) };

            string path = Path.GetFullPath(originalPath);

            if (!GetFilePath(ref path))
            {
                path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), originalPath);
                if (!GetFilePath(ref path))
                    thread.ThrowScriptError("LibraryNotFoundException. " + path + " could not be found.");
            }

            if (Path.GetExtension(path) == ".dll")
                ret = LoadAssembly(thread, path, delegetateParamsHelper);
            else
                ret = LoadFile(path, thread);

            thread.CurrentNode = Parent;
            return ret;
        }

        private bool GetFilePath(ref string path)
        {
            var tPath = path;
            var hasExtension = Path.GetExtension(tPath).Length > 0;

            if (!hasExtension)
            {
                if (File.Exists(tPath + ".dll"))
                    tPath += ".dll";
                else if (File.Exists(tPath + ".cal"))
                    tPath += ".cal";
            }

            if (!File.Exists(tPath))
                return false;

            path = tPath;
            return true;
        }

        private bool LoadFile(string path, ScriptThread thread)
        {
            bool error = false;
            string fileContent = File.ReadAllText(path);
            var back = thread.App.Status;
            var pamode = thread.App.ParserMode;
            thread.App.ParserMode = ParseMode.File;

            thread.App.Evaluate(fileContent, path);

            switch (thread.App.Status)
            {
                case AppStatus.SyntaxError:
                    foreach (var err in thread.App.GetParserMessages())
                    {
                        thread.App.WriteLine($"[{err.Level}] {err.Location.ToUiString()} in {path}: {err.Message}");
                    }
                    error = true;
                    break;
                case AppStatus.Crash:
                case AppStatus.RuntimeError:
                    var ex = thread.App.LastException;
                    var screx = ex as ScriptException;
                    if (screx != null)
                    {
                        thread.App.WriteLine(screx.ToString());
                        if (screx.InnerException != null)
                            thread.App.WriteLine(screx.InnerException.ToString());
                    }
                    else
                        thread.App.WriteLine(ex.Message);
                    error = true;
                    break;
                default:
                    break;
            }
            thread.App.Status = back;
            thread.App.ParserMode = pamode;
            return !error;
        }

        private static bool LoadAssembly(ScriptThread thread, string path, List<Type> delegetateParamsHelper)
        {
            bool error = false;
            try
            {
                var assembly = Assembly.LoadFile(path);

                var methods = assembly.GetTypes()
                                        .SelectMany(t => t.GetMethods())
                                        .Where(m => m.GetCustomAttributes(typeof(CalcCallableMethodAttribute), false).Length > 0
                                                    && m.ReturnType == typeof(object)
                                                    && m.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(delegetateParamsHelper))
                                        .ToArray();

                foreach (var method in methods)
                {
                    CalcCallableMethodAttribute attr = method.GetCustomAttribute<CalcCallableMethodAttribute>();
                    thread.Runtime.BuiltIns.AddMethod((BuiltInMethod)method.CreateDelegate(typeof(BuiltInMethod)),
                        attr.Name, attr.ParameterCount,
                        (attr.ParameterNames == null ? null : string.Join(",", attr.ParameterNames)));
                }
            }
            catch (Exception)
            {
                error = true;
            }

            return !error;
        }
    }
}
