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
            Console.WriteLine(string.Join("\t", parameters.Select(x => x?.ToString() ?? "NULL")));
            return null;
        }

        [CalcCallableMethod("ScriptPrint", -1)]
        public static object ScriptPrint(ScriptThread thread, object instance, object[] parameters)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
            thread.App.WriteLine(string.Join("\t", parameters.Select(x => x?.ToString() ?? "NULL")));
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
    }
}
