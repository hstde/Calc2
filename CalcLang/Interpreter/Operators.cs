using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public delegate object UnaryOperatorMethod(object arg);
    public delegate object BinaryOperatorMethod(object arg1, object arg2);

    public struct OperatorDispatchKey
    {
        public static readonly OperatorDispatchKeyComparer Comparer = new OperatorDispatchKeyComparer();
        public readonly ExpressionType Op;
        public readonly Type Arg1Type;
        public readonly Type Arg2Type;
        public readonly int HashCode;

        public OperatorDispatchKey(ExpressionType op, Type arg1Type, Type arg2Type)
        {
            Op = op;
            Arg1Type = arg1Type;
            Arg2Type = arg2Type;
            int h0 = (int)Op;
            int h1 = Arg1Type.GetHashCode();
            int h2 = Arg2Type?.GetHashCode() ?? 0;
            HashCode = unchecked(h0 << 8 ^ h1 << 4 ^ h2);
        }

        public OperatorDispatchKey(ExpressionType op, Type argType) : this(op, argType, null)
        {
        }

        public override int GetHashCode() => HashCode;

        public override string ToString() => Op + "(" + Arg1Type + ", " + Arg2Type + ")";
    }

    public class OperatorDispatchKeyComparer : IEqualityComparer<OperatorDispatchKey>
    {
        public bool Equals(OperatorDispatchKey x, OperatorDispatchKey y)
            => x.HashCode == y.HashCode && x.Op == y.Op && x.Arg1Type == y.Arg1Type && x.Arg2Type == y.Arg2Type;
        public int GetHashCode(OperatorDispatchKey obj) => obj.HashCode;
    }

    [Serializable]
    public class TypeConverterTable : Dictionary<OperatorDispatchKey, UnaryOperatorMethod>
    {
        public TypeConverterTable(int capacity) : base(capacity, OperatorDispatchKey.Comparer)
        {
        }
    }

    [Serializable]
    public class OperatorImplementationTable : Dictionary<OperatorDispatchKey, OperatorImplementation>
    {
        public OperatorImplementationTable(int capacity) : base(capacity, OperatorDispatchKey.Comparer)
        {
        }
    }
}
