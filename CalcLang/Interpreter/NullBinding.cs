using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class NullBinding : Binding
    {
        public NullBinding() : base("Null", BindingTargetType.SpecialForm)
        {
            GetValueRef = t => t.Runtime.NullValue;
            SetValueRef = (t, v) => t.ThrowScriptError("Tried setting a NullBinding.");
        }
    }
}
