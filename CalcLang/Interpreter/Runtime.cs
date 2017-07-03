using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public interface IBindingSource
    {
        Binding Bind(BindingRequest request);
    }

    public class NullClass
    {
        private readonly bool isNull;
        protected NullClass(bool isNull) { this.isNull = isNull; }
        public override string ToString() => isNull ? "Null" : "NonNull";

        public override bool Equals(object obj)
        {
            if (this.isNull && obj == null) return true;
            var other = obj as NullClass;
            return isNull == other?.isNull;
        }

        public override int GetHashCode() => isNull.GetHashCode();

        public static NullClass NullValue = new NullClass(true);
        public static NullClass NonNullValue = new NullClass(false);
    }

    public class BindingSourceList : List<IBindingSource>
    {
    }

    [Serializable]
    public class BindingSourceTable : Dictionary<string, IBindingSource>, IBindingSource
    {
        public Binding Bind(BindingRequest request)
        {
            IBindingSource target;
            if (TryGetValue(request.Symbol, out target))
                return target.Bind(request);
            return null;
        }

        public BindingTargetInfo AddMethod(BuiltInMethod method, string methodName, int paramCount = -1, string paramNames = null)
        {
            var callTarget = new BuiltInCallTarget(method, methodName, paramCount, paramNames);
            BuiltInCallableTargetInfo targetInfo = null;
            IBindingSource source;

            if (this.TryGetValue(methodName, out source))
            {
                targetInfo = source as BuiltInCallableTargetInfo;
                var mTable = (targetInfo.BindingInstance as ConstantBinding).Target as MethodTable;
                mTable.Add(callTarget);
            }
            else
            {
                var mTable = new MethodTable(methodName);
                mTable.Add(callTarget);
                targetInfo = new BuiltInCallableTargetInfo(mTable);
                this[methodName] = targetInfo;
            }

            return targetInfo;
        }
    }

    public class Runtime : IBindingSource
    {
        public readonly Irony.Parsing.LanguageData Language;
        public BindingSourceTable BuiltIns;
        public BindingSourceTable ExtensionFunctions;
        public readonly OperatorImplementationTable OperatorImplementations = new OperatorImplementationTable(2000);

        public NullClass NullValue { get; protected set; }

        public NullClass NonNullValue { get; protected set; }

        public Runtime(Irony.Parsing.LanguageData language)
        {
            Language = language;
            NullValue = NullClass.NullValue;
            NonNullValue = NullClass.NonNullValue;
            BuiltIns = new BindingSourceTable();
            ExtensionFunctions = new BindingSourceTable();
            Init();
        }

        public void Init()
        {
            InitBuiltInMethods();
            InitOperatorImplementations();
        }

        public void InitBuiltInMethods()
        {
            /*BuiltIns.AddMethod(BuiltInMethods.GetType, ".GetType", 0);
            BuiltIns.AddMethod(BuiltInMethods.Print, "_Built_In_Print");
            BuiltIns.AddMethod(BuiltInMethods.Print, "print");
            BuiltIns.AddMethod(BuiltInMethods.CharOut, "_Built_In_Char_Out", 1);
            BuiltIns.AddMethod(BuiltInMethods.StringAsTable, ".AsTable", 0);
            BuiltIns.AddMethod(BuiltInMethods.TableAsString, ".AsString", 0);
            BuiltIns.AddMethod(BuiltInMethods.Fault, "fault");*/
        }

        public bool IsTrue(object value)
        {
            if (value is bool)
                return (bool)value;
            if (value is int)
                return (int)value != 0;
            if (Equals(value, NullValue))
                return false;
            if (value is double)
                return (double)value != 0;
            return value != null;
        }

        public Binding Bind(BindingRequest request)
        {
            var symbol = request.Symbol;
            var mode = request.Flags;
            if ((mode & BindingRequestFlags.Write) != 0)
            {
                return BindSymbolForWrite(request);
            }
            else if ((mode & BindingRequestFlags.Read) != 0)
            {
                return BindSymbolForRead(request);
            }
            else
            {
                request.Thread.ThrowScriptError("Invalid binding request!");
                return null;
            }
        }

        public Binding BindSymbolForWrite(BindingRequest request)
        {
            var currScope = request.Thread.CurrentScope;
            var symbol = request.Symbol;
            var existingSlot = currScope.ScopeInfo.GetSlot(request.Symbol);

            if ((request.Flags & BindingRequestFlags.ExistingOrNew) != 0)
            {
                if ((request.Flags & BindingRequestFlags.Existing) != 0)
                {
                    var scope = currScope;
                    do
                    {
                        existingSlot = currScope.ScopeInfo.GetSlot(symbol);
                        if (existingSlot != null)
                            return new SlotBinding(existingSlot, request.FromNode, request.FromScopeInfo);
                        currScope = currScope.Parent;
                    } while (currScope != null);
                    currScope = scope;

                    if ((request.Flags & BindingRequestFlags.Extern) != 0)
                    {
                        var builtIn = BuiltIns.Bind(request);
                        if (builtIn != null) return builtIn;
                    }
                }

                if ((request.Flags & BindingRequestFlags.NewOnly) != 0)
                {
                    if (existingSlot != null)
                        ThrowScriptError("Var {0} is already defined!", symbol);

                    var newSlot = currScope.AddSlot(request.Symbol);
                    return new SlotBinding(newSlot, request.FromNode, request.FromScopeInfo);
                }
            }

            return null;
        }

        public Binding BindSymbolForRead(BindingRequest request)
        {
            var symbol = request.Symbol;
            var currScope = request.Thread.CurrentScope;
            do
            {
                var existingSlot = currScope.ScopeInfo.GetSlot(symbol);
                if (existingSlot != null)
                    return new SlotBinding(existingSlot, request.FromNode, request.FromScopeInfo);
                currScope = currScope.Parent;
            } while (currScope != null);

            if ((request.Flags & BindingRequestFlags.Extern) != 0)
            {
                var builtIn = BuiltIns.Bind(request);
                if (builtIn != null) return builtIn;
            }

            if ((request.Flags & BindingRequestFlags.NullOk) != 0)
                return new NullBinding();

            return null;
        }

        public object ExecuteBinaryOperator(ExpressionType op, object arg1, object arg2, ref OperatorImplementation previousUsed)
        {
            Type arg1Type, arg2Type;
            try
            {
                arg1Type = arg1.GetType();
                arg2Type = arg2.GetType();
            }
            catch (NullReferenceException)
            {
                CheckUnassigned(arg1);
                CheckUnassigned(arg2);
                throw;
            }

            var currentImpl = previousUsed;
            if (currentImpl != null && (arg1Type != currentImpl.Key.Arg1Type || arg2Type != currentImpl.Key.Arg2Type))
                currentImpl = null;

            OperatorDispatchKey key;
            if (currentImpl == null)
            {
                key = new OperatorDispatchKey(op, arg1Type, arg2Type);
                if (!OperatorImplementations.TryGetValue(key, out currentImpl))
                    throw new ScriptException($"Op(<{arg1Type.Name}> <{op}> <{arg2Type.Name}>) not defined!");
            }

            try
            {
                previousUsed = currentImpl;
                return currentImpl.EvaluateBinary(arg1, arg2);
            }
            catch (OverflowException)
            {
                if (currentImpl.OverflowHandler == null) throw;
                previousUsed = currentImpl.OverflowHandler;
                return ExecuteBinaryOperator(op, arg1, arg2, ref previousUsed);
            }
            catch (IndexOutOfRangeException)
            {
                throw;
            }
        }

        public object ExecuteUnaryOperator(ExpressionType op, object arg1, ref OperatorImplementation previousUsed)
        {
            Type arg1Type;
            try
            {
                arg1Type = arg1.GetType();
            }
            catch (NullReferenceException)
            {
                CheckUnassigned(arg1);
                throw;
            }

            OperatorDispatchKey key;
            var currentImpl = previousUsed;
            if (currentImpl != null && arg1Type != currentImpl.Key.Arg1Type)
                currentImpl = null;

            if (currentImpl == null)
            {
                key = new OperatorDispatchKey(op, arg1Type);
                if (!OperatorImplementations.TryGetValue(key, out currentImpl))
                    throw new ScriptException($"Op(<{op}> <{arg1Type.Name}>) not defined!");
            }

            try
            {
                previousUsed = currentImpl;
                return currentImpl.Arg1Converter(arg1);
            }
            catch (OverflowException)
            {
                if (currentImpl.OverflowHandler == null)
                    throw;
                previousUsed = currentImpl.OverflowHandler;
                return ExecuteUnaryOperator(op, arg1, ref previousUsed);
            }
        }

        private void CheckUnassigned(object value)
        {
            if (value == null)
                throw new Exception("Variable unassigned.");
        }

        internal protected void ThrowError(string message, params object[] args)
        {
            if (args?.Length > 0)
                message = string.Format(message, args);
            throw new Exception(message);
        }

        internal protected void ThrowScriptError(string message, params object[] args)
        {
            if (args?.Length > 0)
                message = string.Format(message, args);
            throw new ScriptException(message);
        }

        private static readonly ExpressionType[] overflowOperators = new ExpressionType[]
        {
            ExpressionType.Add, ExpressionType.AddChecked, ExpressionType.Subtract, ExpressionType.SubtractChecked,
            ExpressionType.Multiply, ExpressionType.MultiplyChecked, ExpressionType.Power
        };

        protected void InitOperatorImplementations()
        {
            InitTypeConverters();
            InitBinaryOperatorImplementationsForMatchedTypes();
            InitUnaryOperatorImplementations();
            CreateBinaryOperatorImplementationsForMismatchedTypes();
            CreateOverflowHandlers();
        }

        protected OperatorImplementation AddConverter(Type fromType, Type toType, UnaryOperatorMethod method)
        {
            var key = new OperatorDispatchKey(ExpressionType.ConvertChecked, fromType, toType);
            var impl = new OperatorImplementation(key, toType, method);
            OperatorImplementations[key] = impl;
            return impl;
        }

        protected OperatorImplementation AddBinary(ExpressionType op, Type baseType, BinaryOperatorMethod binaryMethod)
            => AddBinary(op, baseType, binaryMethod, null);

        protected OperatorImplementation AddBinary(ExpressionType op, Type commonType, BinaryOperatorMethod binaryMethod,
            UnaryOperatorMethod resultConverter)
        {
            var key = new OperatorDispatchKey(op, commonType, commonType);
            var impl = new OperatorImplementation(key, commonType, binaryMethod, null, null, resultConverter);
            OperatorImplementations[key] = impl;
            return impl;
        }

        protected OperatorImplementation AddUnary(ExpressionType op, Type commonType, UnaryOperatorMethod unaryMethod)
        {
            var key = new OperatorDispatchKey(op, commonType);
            var impl = new OperatorImplementation(key, commonType, null, unaryMethod, null, null);
            OperatorImplementations[key] = impl;
            return impl;
        }

        public void InitTypeConverters()
        {
            Type targetType = typeof(string);
            AddConverter(typeof(long), targetType, ConvertAnyToString);
            AddConverter(typeof(bool), targetType, ConvertAnyToString);
            AddConverter(typeof(char), targetType, ConvertAnyToString);
            AddConverter(typeof(DataTable), targetType, ConvertAnyToString);
            AddConverter(typeof(double), targetType, ConvertAnyToString);
            AddConverter(typeof(decimal), targetType, ConvertAnyToString);
            AddConverter(typeof(int), targetType, ConvertAnyToString);
            AddConverter(typeof(NullClass), targetType, value => null);

            targetType = typeof(double);
            AddConverter(typeof(char), targetType, value => (double)(char)value);
            AddConverter(typeof(int), targetType, value => (double)(int)value);
            AddConverter(typeof(decimal), targetType, value => (double)(decimal)value);
            AddConverter(typeof(long), targetType, value => (double)(long)value);

            targetType = typeof(decimal);
            AddConverter(typeof(char), targetType, v => (decimal)(char)v);
            AddConverter(typeof(int), targetType, v => (decimal)(int)v);
            AddConverter(typeof(double), targetType, v => (decimal)(double)v);
            AddConverter(typeof(long), targetType, value => (decimal)(long)value);

            targetType = typeof(long);
            AddConverter(typeof(char), targetType, v => (long)(char)v);
            AddConverter(typeof(int), targetType, v => (long)(int)v);

            targetType = typeof(NullClass);
            AddConverter(typeof(bool), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(char), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(DataTable), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(double), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(decimal), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(int), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(long), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(string), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(BuiltInCallTarget), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(Closure), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(object), targetType, value => value == null ? NullValue : NonNullValue);
            AddConverter(typeof(MethodTable), targetType, value => value == null ? NullValue : NonNullValue);
        }

        public static object ConvertAnyToString(object value) => value == null ? string.Empty : value.ToString();

        public void InitBinaryOperatorImplementationsForMatchedTypes()
        {
            ExpressionType op = ExpressionType.AddChecked;
            AddBinary(op, typeof(int), (x, y) => checked((int)x + (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x + (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x + (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x + (decimal)y);
            AddBinary(op, typeof(string), (x, y) => (string)x + (string)y);
            AddBinary(op, typeof(char), (x, y) => (char)x + (char)y);

            op = ExpressionType.SubtractChecked;
            AddBinary(op, typeof(int), (x, y) => checked((int)x - (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x - (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x - (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x - (decimal)y);
            AddBinary(op, typeof(char), (x, y) => (char)x - (char)y);

            op = ExpressionType.MultiplyChecked;
            AddBinary(op, typeof(int), (x, y) => checked((int)x * (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x * (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x * (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x * (decimal)y);
            AddBinary(op, typeof(char), (x, y) => (char)x * (char)y);

            op = ExpressionType.Divide;
            AddBinary(op, typeof(int), (x, y) => checked((int)x / (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x / (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x / (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x / (decimal)y);
            AddBinary(op, typeof(char), (x, y) => (char)x / (char)y);

            op = ExpressionType.Modulo;
            AddBinary(op, typeof(int), (x, y) => checked((int)x % (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x % (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x % (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x % (decimal)y);
            AddBinary(op, typeof(char), (x, y) => (char)x % (char)y);

            op = ExpressionType.LessThan;
            AddBinary(op, typeof(int), (x, y) => checked((int)x < (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x < (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x < (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x < (decimal)y);
            //AddBinary(op, typeof(string), (x, y) => ((string)x)[0] < ((string)y)[0]);
            AddBinary(op, typeof(char), (x, y) => (char)x < (char)y);

            op = ExpressionType.GreaterThan;
            AddBinary(op, typeof(int), (x, y) => checked((int)x > (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x > (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x > (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x > (decimal)y);
            //AddBinary(op, typeof(string), (x, y) => ((string)x)[0] > ((string)y)[0]);
            AddBinary(op, typeof(char), (x, y) => (char)x > (char)y);

            op = ExpressionType.LessThanOrEqual;
            AddBinary(op, typeof(int), (x, y) => checked((int)x <= (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x <= (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x <= (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x <= (decimal)y);
            //AddBinary(op, typeof(string), (x, y) => ((string)x)[0] <= ((string)y)[0]);
            AddBinary(op, typeof(char), (x, y) => (char)x <= (char)y);

            op = ExpressionType.GreaterThanOrEqual;
            AddBinary(op, typeof(int), (x, y) => checked((int)x >= (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x >= (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x >= (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x >= (decimal)y);
            //AddBinary(op, typeof(string), (x, y) => ((string)x)[0] >= ((string)y)[0]);
            AddBinary(op, typeof(char), (x, y) => (char)x >= (char)y);

            op = ExpressionType.Equal;
            AddBinary(op, typeof(int), (x, y) => checked((int)x == (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x == (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x == (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x == (decimal)y);
            AddBinary(op, typeof(string), (x, y) => string.Equals(x as string, y as string));
            AddBinary(op, typeof(char), (x, y) => (char)x == (char)y);
            AddBinary(op, typeof(NullClass), (x, y) => x.Equals(y));
            AddBinary(op, typeof(bool), (x, y) => (bool)x == (bool)y);

            op = ExpressionType.NotEqual;
            AddBinary(op, typeof(int), (x, y) => checked((int)x != (int)y));
            AddBinary(op, typeof(long), (x, y) => checked((long)x != (long)y));
            AddBinary(op, typeof(double), (x, y) => (double)x != (double)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)x != (decimal)y);
            AddBinary(op, typeof(string), (x, y) => !string.Equals(x as string, y as string));
            AddBinary(op, typeof(char), (x, y) => (char)x != (char)y);
            AddBinary(op, typeof(NullClass), (x, y) => !x.Equals(y));
            AddBinary(op, typeof(bool), (x, y) => (bool)x != (bool)y);

            op = ExpressionType.And;
            AddBinary(op, typeof(bool), (x, y) => (bool)x & (bool)y);
            AddBinary(op, typeof(int), (x, y) => (int)x & (int)y);
            AddBinary(op, typeof(long), (x, y) => (long)x & (long)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)((long)((decimal)x) & (long)((decimal)y)));
            AddBinary(op, typeof(char), (x, y) => (char)x & (char)y);

            op = ExpressionType.Or;
            AddBinary(op, typeof(bool), (x, y) => (bool)x | (bool)y);
            AddBinary(op, typeof(int), (x, y) => (int)x | (int)y);
            AddBinary(op, typeof(long), (x, y) => (long)x | (long)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)((long)((decimal)x) | (long)((decimal)y)));
            AddBinary(op, typeof(char), (x, y) => (char)x | (char)y);

            op = ExpressionType.LeftShift;
            AddBinary(op, typeof(int), (x, y) => (int)x << (int)y);
            AddBinary(op, typeof(long), (x, y) => (long)x << (int)(long)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)((long)(decimal)x << (int)(decimal)y));
            AddBinary(op, typeof(char), (x, y) => (char)x << (char)y);

            op = ExpressionType.RightShift;
            AddBinary(op, typeof(int), (x, y) => (int)x >> (int)y);
            AddBinary(op, typeof(long), (x, y) => (long)x >> (int)(long)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)((long)(decimal)x >> (int)(decimal)y));
            AddBinary(op, typeof(char), (x, y) => (char)x >> (char)y);

            op = ExpressionType.ExclusiveOr;
            AddBinary(op, typeof(bool), (x, y) => (bool)x ^ (bool)y);
            AddBinary(op, typeof(int), (x, y) => (int)x ^ (int)y);
            AddBinary(op, typeof(long), (x, y) => (long)x ^ (long)y);
            AddBinary(op, typeof(decimal), (x, y) => (decimal)((long)((decimal)x) ^ (long)((decimal)y)));
            AddBinary(op, typeof(char), (x, y) => (char)x ^ (char)y);
        }

        public void InitUnaryOperatorImplementations()
        {
            var op = ExpressionType.UnaryPlus;

            AddUnary(op, typeof(int), x => +(int)x);
            AddUnary(op, typeof(long), x => +(long)x);
            AddUnary(op, typeof(double), x => +(double)x);
            AddUnary(op, typeof(decimal), x => +(decimal)x);
            AddUnary(op, typeof(char), x => +(char)x);

            op = ExpressionType.Negate;
            AddUnary(op, typeof(int), x => -(int)x);
            AddUnary(op, typeof(long), x => -(long)x);
            AddUnary(op, typeof(double), x => -(double)x);
            AddUnary(op, typeof(decimal), x => -(decimal)x);
            AddUnary(op, typeof(char), x => -(char)x);

            op = ExpressionType.Not;
            AddUnary(op, typeof(bool), x => !(bool)x);
            AddUnary(op, typeof(int), x => ~(int)x);
            AddUnary(op, typeof(long), x => ~(long)x);
            AddUnary(op, typeof(char), x => ~(char)x);
        }

        public void CreateBinaryOperatorImplementationsForMismatchedTypes()
        {
            var allTypes = new HashSet<Type>();
            var allBinOps = new HashSet<ExpressionType>();
            foreach (var kv in OperatorImplementations)
            {
                allTypes.Add(kv.Key.Arg1Type);
                if (kv.Value.BaseBinaryMethod != null)
                {
                    allBinOps.Add(kv.Key.Op);
                }
            }
            foreach (var arg1Type in allTypes)
            {
                foreach (var arg2Type in allTypes)
                {
                    foreach (var op in allBinOps)
                        CreateBinaryOperatorImplementation(op, arg1Type, arg2Type);
                }
            }
        }

        private OperatorImplementation CreateBinaryOperatorImplementation(ExpressionType op, Type arg1Type, Type arg2Type)
        {
            Type commonType = GetCommonTypeForOperator(op, arg1Type, arg2Type);
            if (commonType == null)
                return null;

            var baseImpl = FindBaseImplementation(op, commonType);
            if (baseImpl == null)
            {
                commonType = GetUpType(commonType);
                if (commonType == null)
                    return null;
                baseImpl = FindBaseImplementation(op, commonType);
            }
            if (baseImpl == null)
                return null;

            var impl = CreateBinaryOperatorImplementation(op, arg1Type, arg2Type, commonType, baseImpl.BaseBinaryMethod, baseImpl.ResultConverter);
            OperatorImplementations[impl.Key] = impl;
            return impl;
        }

        protected OperatorImplementation CreateBinaryOperatorImplementation(ExpressionType op, Type arg1Type, Type arg2Type,
                   Type commonType, BinaryOperatorMethod method, UnaryOperatorMethod resultConverter)
        {
            OperatorDispatchKey key = new OperatorDispatchKey(op, arg1Type, arg2Type);
            UnaryOperatorMethod arg1Converter = arg1Type == commonType ? null : GetConverter(arg1Type, commonType);
            UnaryOperatorMethod arg2Converter = arg2Type == commonType ? null : GetConverter(arg2Type, commonType);
            var impl = new OperatorImplementation(key, commonType, method,
                arg1Converter, arg2Converter, resultConverter);
            return impl;
        }

        protected void CreateOverflowHandlers()
        {
            foreach (var impl in OperatorImplementations.Values)
            {
                if (!CanOverflow(impl))
                    continue;
                var key = impl.Key;
                var upType = GetUpType(impl.CommonType);
                if (upType == null)
                    continue;
                var upBaseImpl = FindBaseImplementation(key.Op, upType);
                if (upBaseImpl == null)
                    continue;
                impl.OverflowHandler = CreateBinaryOperatorImplementation(key.Op, key.Arg1Type, key.Arg2Type, upType,
                    upBaseImpl.BaseBinaryMethod, upBaseImpl.ResultConverter);
            }
        }

        private UnaryOperatorMethod GetConverter(Type fromType, Type toType)
        {
            if (fromType == toType)
                return x => x;
            var key = new OperatorDispatchKey(ExpressionType.ConvertChecked, fromType, toType);
            OperatorImplementation impl;
            if (!OperatorImplementations.TryGetValue(key, out impl))
                return null;
            return impl.Arg1Converter;
        }

        private Type GetUpType(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(decimal)) return typeof(double);
            return null;
        }

        private OperatorImplementation FindBaseImplementation(ExpressionType op, Type commonType)
        {
            var baseKey = new OperatorDispatchKey(op, commonType, commonType);
            OperatorImplementation baseImpl;
            OperatorImplementations.TryGetValue(baseKey, out baseImpl);
            return baseImpl;
        }

        private static readonly Irony.TypeList typesSequence = new Irony.TypeList(
            typeof(char), typeof(int), typeof(long), typeof(double), typeof(decimal), typeof(bool),
            typeof(DataTable), typeof(BuiltInCallTarget), typeof(Closure), typeof(MethodTable), typeof(NullClass), typeof(string));

        private Type GetCommonTypeForOperator(ExpressionType op, Type arg1Type, Type arg2Type)
        {
            if (arg1Type == arg2Type)
                return arg1Type;

            var index1 = typesSequence.IndexOf(arg1Type);
            var index2 = typesSequence.IndexOf(arg2Type);
            if (index1 >= 0 && index2 >= 0)
                return typesSequence[Math.Max(index1, index2)];
            return null;
        }

        private static bool CanOverflow(OperatorImplementation impl)
        {
            if (!CanOverflow(impl.Key.Op))
                return false;
            if (impl.CommonType == typeof(double) || impl.CommonType == typeof(float))
                return false;
            return true;
        }

        private static bool CanOverflow(ExpressionType expression) => overflowOperators.Contains(expression);
    }
}
