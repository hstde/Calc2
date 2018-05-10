using System;
using System.Linq;

namespace CalcLang.Interpreter
{
    public class CalcCallableMethodAttribute : Attribute
    {
        public string Name { get; }
        public int ParameterCount { get; }
        public string[] ParameterNames { get; }
        public TypeInfo[] ParamTypes { get; }

        private CalcCallableMethodAttribute() { }
        public CalcCallableMethodAttribute(string methodName, int parameterCount = 0, TypeInfo[] types = null, string paramNames = null)
        {
            Name = methodName;
            ParameterCount = parameterCount;
            ParamTypes = types;
            if (!string.IsNullOrEmpty(paramNames))
                ParameterNames = paramNames.Split(',').Select(x => x.Trim()).ToArray();
        }
    }
}
