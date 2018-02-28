using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using System.Globalization;

namespace Base
{
    public static class Debug
    {
        [CalcCallableMethod("DebugPrint", -1)]
        public static object Print(ScriptThread thread, object thisRef, object[] parameters)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
            Console.WriteLine(string.Join("\t", (parameters[0] as DataTable).Select(x => x?.ToString() ?? "NULL")));
            return null;
        }

        [CalcCallableMethod("ScriptPrint", -1)]
        public static object ScriptPrint(ScriptThread thread, object instance, object[] parameters)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
            thread.App.WriteLine(string.Join("\t", (parameters[0] as DataTable).Select(x => x?.ToString() ?? "NULL")));
            return null;
        }

        [CalcCallableMethod("CollectAll")]
        public static object CollectAll(ScriptThread t, object th, object[] par)
        {
            decimal coll = 0;
            int pre = GC.CollectionCount(0);
            GC.Collect(0);
            coll += GC.CollectionCount(0) - pre;

            pre = GC.CollectionCount(1);
            GC.Collect(1);
            coll += GC.CollectionCount(1) - pre;

            pre = GC.CollectionCount(2);
            GC.Collect(2);
            coll += GC.CollectionCount(2) - pre;

            return coll;
        }

        [CalcCallableMethod("GetTotalMemory")]
        public static object GetTotalMemory(ScriptThread t, object thisRef, object[] parmametrs)
        {
            return GC.GetTotalMemory(true);
        }

        [CalcCallableMethod("GetFunctionInfo")]
        public static object GetFunctionInfo(ScriptThread thread, object thisRef, object[] parameters)
        {
            DataTable GetSingleInfo(ICallTarget callTarget)
            {
                var result = new DataTable();
                var fi = callTarget.GetFunctionInfo();

                result.SetString(thread, "Name", fi.Name);
                result.SetString(thread, "ParameterCount", fi.ParamCount);
                result.SetString(thread, "HasParamsArgument", fi.HasParamsArg);
                result.SetString(thread, "ParameterNames", new DataTable(fi.ParamNames, thread));
                result.SetString(thread, "ParameterTypes", new DataTable(fi.ParamTypes.Select(e => e.ToString()), thread));

                return result;
            }

            var target = thisRef as ICallTarget;
            var mTable = thisRef as MethodTable;

            var res = new DataTable();

            if(target != null)
            {
                res.SetInt(thread, 0, GetSingleInfo(target));
            }
            else if(mTable != null)
            {
                int i = 0;
                foreach(var e in mTable.AsEnumerable())
                {
                    res.SetInt(thread, i, GetSingleInfo(e));
                    i++;
                }
            }

            return res;
        }
    }
}
