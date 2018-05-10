using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public enum SlotType
    {
        Value,
        Parameter,
        Function,
        Closure
    }

    public class SlotInfo
    {
        public readonly ScopeInfo ScopeInfo;
        public readonly SlotType Type;
        public readonly TypeInfo ValueType;
        public readonly string Name;
        public readonly int Index;
        public bool IsPublic = true;

        internal SlotInfo(ScopeInfo scopeInfo, SlotType type, TypeInfo valueType, string name, int index)
        {
            ScopeInfo = scopeInfo;
            Type = type;
            ValueType = valueType;
            Name = name;
            Index = index;
        }
    }

    [Serializable]
    public class SlotInfoDictionary : Dictionary<string, SlotInfo>
    {
        public SlotInfoDictionary() : base(32)
        {
        }
    }
}
