using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Ast;

namespace CalcLang.Interpreter
{
    public delegate object BuiltInMethod(ScriptThread thread, object thisRef, object[] args);

    public enum BindingTargetType
    {
        Slot,
        BuiltInObject,
        SpecialForm,
        ClrInterop
    }

    public class BindingTargetInfo
    {
        public readonly string Symbol;
        public readonly BindingTargetType Type;

        public BindingTargetInfo(string symbol, BindingTargetType type)
        {
            Symbol = symbol;
            Type = type;
        }

        public override string ToString() => Symbol + "/" + Type.ToString();
    }

    public class Binding
    {
        public readonly BindingTargetInfo TargetInfo;
        public EvaluateMethod GetValueRef;
        public ValueSetterMethod SetValueRef;

        public bool IsConstant
        {
            get;
            protected set;
        }

        public Binding(BindingTargetInfo targetInfo)
        {
            TargetInfo = targetInfo;
        }
        public Binding(string symbol, BindingTargetType targetType)
        {
            TargetInfo = new BindingTargetInfo(symbol, targetType);
        }

        public override string ToString() => $"{{Binding to {TargetInfo.ToString()}}}";
    }

    public class ConstantBinding : Binding
    {
        public object Target;

        public ConstantBinding(object target, BindingTargetInfo targetInfo) : base(targetInfo)
        {
            Target = target;
            GetValueRef = GetValue;
            IsConstant = true;
        }

        public object GetValue(ScriptThread thread) => Target;
    }


    public class BuiltInCallableTargetInfo : BindingTargetInfo, IBindingSource
    {
        public Binding BindingInstance;

        public BuiltInCallableTargetInfo(BuiltInCallTarget target) : base(target.Name, BindingTargetType.BuiltInObject)
        {
            BindingInstance = new ConstantBinding(target, this);
        }

        public BuiltInCallableTargetInfo(MethodTable target) : base(target.Name, BindingTargetType.BuiltInObject)
        {
            BindingInstance = new ConstantBinding(target, this);
        }

        public Binding Bind(BindingRequest request) => BindingInstance;
    }

    public class BuiltInCallTarget : ICallTarget
    {
        public MethodTable MethodTable { get; set; }

        public string Name;
        public readonly BuiltInMethod Method;
        public readonly int ParamCount;
        public string[] ParamNames;
        public bool HasParamsArg;

        public BuiltInCallTarget(BuiltInMethod method, string name, int paramCount, string paramNames = null)
        {
            Method = method;
            Name = name;
            ParamCount = paramCount;
            if (!string.IsNullOrEmpty(paramNames))
                ParamNames = paramNames.Split(',');
        }

        public object Call(ScriptThread thread, object thisRef, object[] parameters) => Method(thread, thisRef, parameters);

        public FunctionInfo GetFunctionInfo() => new FunctionInfo(Name, ParamCount, ParamNames, HasParamsArg);

        public override string ToString() => "Function";

        public int GetParameterCount() => ParamCount;

        public bool GetHasParamsArg() => HasParamsArg;
    }
}
