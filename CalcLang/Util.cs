using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang
{
    public static class Util
    {
        public static void Check(bool condition, string message, params object[] args)
        {
            if (condition) return;
            throw new Exception(string.Format(message, args));
        }
    }
}
