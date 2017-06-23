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
            switch (instance.GetType().Name)
            {
                case "Int32":
                case "Int64":
                case "Int16":
                    return (char)(long)instance;
                default:
                    return '\0';
            }
        }

        [CalcCallableMethod("Break", -1)]
        public static object Break(ScriptThread thread, object thisRef, object[] parameters) => null;

        [CalcCallableMethod("StringJoin", 4)]
        public static object StringJoin(ScriptThread thread, object instance, object[] parameters)
        {
            string ret = null;

            var separator = parameters[0] as string;
            var valueTable = (parameters[1] as DataTable)?.GetIntIndexedDict();
            int startIndex = Convert.ToInt32(parameters[2]);
            int count = Convert.ToInt32(parameters[3]);

            var value = new string[valueTable.Count];

            for (int i = 0; i < value.Length; i++)
                value[i] = valueTable[i].ToString();

            ret = string.Join(separator, value, startIndex, count);

            return ret;
        }

        [CalcCallableMethod("GetTickCount", 0)]
        public static object GetTickCount(ScriptThread thread, object instance, object[] parameters) => (decimal)Environment.TickCount;

        [CalcCallableMethod("GetCurrentTimeTicks", 0)]
        public static object GetCurrentTimeTicks(ScriptThread thread, object instance, object[] parameters) => (decimal)DateTime.Now.Ticks;
    }
}
