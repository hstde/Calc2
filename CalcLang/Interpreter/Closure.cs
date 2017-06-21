using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public interface ICallTarget
    {
        MethodTable MethodTable { get; set; }

        object Call(ScriptThread thread, object thisRef, object[] parameters);
        int GetParameterCount();
        bool GetHasParamsArg();
        FunctionInfo GetFunctionInfo();
    }

    public class Closure : ICallTarget
    {
        public MethodTable MethodTable { get; set; }

        public Scope ParentScope;
        public Ast.LambdaNode Lambda;
        public int ParamCount;
        public bool HasParamsArg;

        public Closure(Scope parentScope, Ast.LambdaNode targetNode)
        {
            ParentScope = parentScope;
            Lambda = targetNode;
            ParamCount = Lambda.Parameters.ParamNames.Length;
            HasParamsArg = Lambda.Parameters.HasParamsArg;
        }
        public object Call(ScriptThread thread, object thisRef, object[] parameters) => Lambda.Call(ParentScope, thread, thisRef, parameters);

        public FunctionInfo GetFunctionInfo()
        {
            var paramNames = Lambda.Parameters.ParamNames;
            var name = Lambda.NameNode != null ? Lambda.NameNode.AsString : "<function>";
            return new FunctionInfo(name, ParamCount, paramNames, HasParamsArg);
        }

        public override string ToString() => "Function";

        public int GetParameterCount() => ParamCount;

        public bool GetHasParamsArg() => HasParamsArg;
    }

    public class FunctionInfo
    {
        public readonly string Name;
        public readonly ICallTarget Target;
        public readonly int ParamCount;
        public readonly string[] ParamNames;
        public SourceInfo CallLocation;
        public readonly bool HasParamsArg;

        public FunctionInfo(string name, int paramCount, string[] paramNames, bool hasParamsArg = false)
        {
            CallLocation = new SourceInfo();
            Name = name;
            ParamCount = paramCount;
            ParamNames = paramNames;
            HasParamsArg = hasParamsArg;
        }

        public override string ToString() => MakeString(Name, ParamCount, ParamNames, CallLocation, HasParamsArg);

        public static string MakeString(string name, int paramCount, string[] paramNames, SourceInfo loc, bool hasParamsArg)
        {
            string[] paramNamesLoc = null;
            if (paramNames != null)
            {
                paramNamesLoc = new string[paramNames.Length];
                Array.Copy(paramNames, paramNamesLoc, paramNames.Length);
            }
            var ret = name;
            var argString = "";
            if (paramNamesLoc == null && paramCount > 0)
                argString = paramCount.ToString();
            else if (paramNamesLoc != null)
            {
                if (hasParamsArg)
                    paramNamesLoc[paramNamesLoc.Length - 1] = "params " + paramNamesLoc[paramNamesLoc.Length - 1];
                argString = string.Join(", ", paramNamesLoc);
            }
            ret += "(" + argString + ")";

            return ret + " in " + loc.FileName + ":Line " + (loc.SourceLocation.Line + 1);
        }
    }
}
