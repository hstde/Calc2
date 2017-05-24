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
        [CalcCallableMethod("__Acos", 1)]
        public static object Acos(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Acos(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Asin", 1)]
        public static object Asin(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Asin(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Atan", 1)]
        public static object Atan(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Atan(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Atan2", 2)]
        public static object Atan2(ScriptThread thread, object instance, object[] parameters) =>
            (decimal)Math.Atan2(Convert.ToDouble(parameters[0]), Convert.ToDouble(parameters[1]));

        [CalcCallableMethod("__Ceiling", 1)]
        public static object Ceiling(ScriptThread thread, object instance, object[] parameters) => Math.Ceiling(Convert.ToDecimal(parameters[0]));

        [CalcCallableMethod("__Cos", 1)]
        public static object Cos(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Cos(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Cosh", 1)]
        public static object Cosh(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Cosh(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Floor", 1)]
        public static object Floor(ScriptThread thread, object instance, object[] parameters) => Math.Floor(Convert.ToDecimal(parameters[0]));

        [CalcCallableMethod("__Sin", 1)]
        public static object Sin(ScriptThread thread, object thisRef, object[] parameters) => (decimal)Math.Sin(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Tan", 1)]
        public static object Tan(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Tan(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Sinh", 1)]
        public static object Sinh(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Sinh(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Tanh", 1)]
        public static object Tanh(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Tanh(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Sqrt", 1)]
        public static object Sqrt(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Sqrt(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Log", 1)]
        public static object Log(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Log(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Log10", 1)]
        public static object Log10(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Log10(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Exp", 1)]
        public static object Exp(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Exp(Convert.ToDouble(parameters[0]));

        [CalcCallableMethod("__Pow", 2)]
        public static object Pow(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Pow(Convert.ToDouble(parameters[0]), Convert.ToDouble(parameters[1]));

        [CalcCallableMethod("__Int", 1)]
        public static object Int(ScriptThread thread, object thisRef, object[] parameters) => (long)Convert.ToDecimal(parameters[0]);

        [CalcCallableMethod("__Round", 1)]
        public static object Round(ScriptThread thread, object instance, object[] parameters) => (decimal)Math.Round(Convert.ToDecimal(parameters[0]));
    }
}
