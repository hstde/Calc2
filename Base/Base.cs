using System;
using System.Linq;
using CalcLang.Interpreter;

namespace Base
{
    internal static class BuiltInMethods
    {
        [CalcCallableMethod(".GetType")]
        public static object GetType(ScriptThread thread, object thisRef, object[] parameters)
        {
            if (thisRef == null || Equals(thisRef, thread.Runtime.NullValue))
                return "Null";

            string result = thisRef.GetType().Name;

            switch (result)
            {
                case "Double":
                case "Decimal":
                case "Int32":
                case "Int64":
                    return "Number";
                case "String":
                    return "String";
                case "Char":
                    return "Char";
                case "DataTable":
                    return "Table";
                case "Closure":
                case "BuiltInCallTarget":
                case "MethodTable":
                    return "Function";
            }

            return result;
        }

        [CalcCallableMethod(".Length")]
        public static object Length(ScriptThread thread, object instance, object[] parameters)
        {
            object result;
            switch (instance.GetType().Name)
            {
                case "DataTable":
                    result = ((DataTable)instance).GetIntIndexedDict().Count;
                    break;
                case "String":
                    result = ((string)instance).Length;
                    break;
                default:
                    if (instance.GetType().IsArray)
                        result = ((Array)instance).Length;
                    else
                        result = -1;
                    break;
            }
            return result;
        }

        [CalcCallableMethod(".ToString")]
        public static object ToString(ScriptThread thread, object thisRef, object[] parameters) => thisRef.ToString();

        [CalcCallableMethod(".ToChar")]
        public static object ToChar(ScriptThread thread, object instance, object[] parameters)
        {
            char result;
            switch(instance.GetType().Name)
            {
                case "Int32":
                case "Int64":
                case "Int16":
                    result = (char)(long)instance;
                    break;
                default:
                    result = '\0';
                    break;
            }
            return result;
        }

        [CalcCallableMethod("Throw", -1)]
        public static object Throw(ScriptThread thread, object thisRef, object[] p)
        {
            string msg = "ScriptException";
            if (p.Length > 0)
                msg += "(" + string.Join(", ", p.Select(x => x?.ToString() ?? "null")) + ")";
            thread.ThrowScriptError(msg, false, null);
            return null;
        }

        [CalcCallableMethod("Break", -1)]
        public static object Break(ScriptThread thread, object thisRef, object[] parameters) => null;
    }
}
