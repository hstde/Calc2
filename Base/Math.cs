using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang;
using CalcLang.Interpreter;

namespace Base
{
    internal static class MathLib
    {
        [CalcCallableMethod("__Int", 1)]
        public static object Int(ScriptThread thread, object thisRef, object[] parameters) => (long)Convert.ToDecimal(parameters[0]);

        [CalcCallableMethod("__Round", 1)]
        public static object Round(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Round(Convert.ToDecimal(parameters[0]));
    }
}
