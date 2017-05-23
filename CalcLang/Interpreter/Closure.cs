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
        FunctionInfo GetFunctionInfo();
    }

    public class Closure : ICallTarget
    {
        public MethodTable MethodTable { get; set; }

        public Scope ParentScope;
        public Ast.LambdaNode Lambda;
        public int ParamCount;

        public Closure(Scope parentScope, Ast.LambdaNode targetNode)
        {
            ParentScope = parentScope;
            Lambda = targetNode;
            ParamCount = Lambda.Parameters.ParamNames.Length;
        }
        public object Call(ScriptThread thread, object thisRef, object[] parameters) => Lambda.Call(ParentScope, thread, thisRef, parameters);

        public FunctionInfo GetFunctionInfo()
        {
            var paramNames = Lambda.Parameters.ParamNames;
            var name = Lambda.NameNode != null ? Lambda.NameNode.AsString : "<function>";
            return new FunctionInfo(name, ParamCount, paramNames);
        }

        public override string ToString() => "Function";

        public int GetParameterCount() => ParamCount;
    }

    public class FunctionInfo
    {
        public readonly string Name;
        public readonly ICallTarget Target;
        public readonly int ParamCount;
        public readonly string[] ParamNames;
        public SourceInfo CallLocation;

        public FunctionInfo(string name, int paramCount, string[] paramNames)
        {
            CallLocation = new SourceInfo();
            Name = name;
            ParamCount = paramCount;
            ParamNames = paramNames;
        }

        public override string ToString() => MakeString(Name, ParamCount, ParamNames, CallLocation);

        public static string MakeString(string name, int paramCount, string[] paramNames, SourceInfo loc)
        {
            var ret = name;
            var argString = "";
            if (paramNames == null && paramCount > 0)
                argString = paramCount.ToString();
            else if (paramNames != null)
                argString = string.Join(", ", paramNames);
            ret += "(" + argString + ")";

            return ret + " in " + loc.FileName + ":Line " + (loc.SourceLocation.Line + 1);
        }
    }
}
