using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class DataTable : IEnumerable<object>
    {
        //metatables?
        private const string KEYS = "Keys";
        private const string LENGTH = "Length";
        private const string GETINDEX = "_[]";
        private const string SETINDEX = "_[]=";
        private const string ADD = "_+_";
        private const string SUB = "_-_";
        private const string MUL = "_*_";
        private const string DIV = "_/_";
        private const string POT = "_**_";
        private const string MOD = "_%_";
        private const string AND = "_&&_";
        private const string OR = "_||_";
        private const string XOR = "_^_";
        private const string LSH = "_<<_";
        private const string RSH = "_>>_";
        private const string UPL = "+_";
        private const string NEG = "-_";
        private const string NOT = "!_";
        private const string EQU = "_==_";
        private const string NEQ = "_!=_";
        private const string GEQ = "_>=_";
        private const string LEQ = "_<=_";
        private const string LES = "_<_";
        private const string GRE = "_>_";

        private readonly List<string> specialIndices = new List<string> { KEYS, LENGTH, GETINDEX, SETINDEX,
            ADD, SUB, MUL, DIV, POT, MOD, AND, OR, XOR, LSH, RSH, UPL, NEG, NOT, EQU, NEQ, GEQ, LEQ, LES, GRE };
        private readonly List<string> operatorIndices = new List<string> { ADD, SUB, MUL, DIV, POT, MOD, AND, OR, XOR, LSH, RSH, UPL, NEG, NOT, EQU, NEQ, GEQ, LEQ, LES, GRE };
        private readonly List<ExpressionType> unary = new List<ExpressionType> { ExpressionType.UnaryPlus, ExpressionType.Negate, ExpressionType.Not };

        private Dictionary<string, object> stringIndexed;
        private readonly Dictionary<int, object> intIndexed;

        private bool indexerLocked = false;
        private ICallTarget indexGetter = null;
        private ICallTarget indexSetter = null;

        private Dictionary<ExpressionType, ICallTarget> operators = new Dictionary<ExpressionType, ICallTarget>();

        private static readonly Dictionary<string, ExpressionType> operatorType = new Dictionary<string, ExpressionType>
        {
            [ADD] = ExpressionType.AddChecked,
            [SUB] = ExpressionType.SubtractChecked,
            [MUL] = ExpressionType.MultiplyChecked,
            [DIV] = ExpressionType.Divide,
            [POT] = ExpressionType.Power,
            [MOD] = ExpressionType.Modulo,
            [AND] = ExpressionType.And,
            [OR] = ExpressionType.Or,
            [XOR] = ExpressionType.ExclusiveOr,
            [LSH] = ExpressionType.LeftShift,
            [RSH] = ExpressionType.RightShift,
            [UPL] = ExpressionType.UnaryPlus,
            [NEG] = ExpressionType.Negate,
            [NOT] = ExpressionType.Not,
            [EQU] = ExpressionType.Equal,
            [NEQ] = ExpressionType.NotEqual,
            [GEQ] = ExpressionType.GreaterThanOrEqual,
            [LEQ] = ExpressionType.LessThanOrEqual,
            [LES] = ExpressionType.LessThan,
            [GRE] = ExpressionType.GreaterThan
        };

        public long Length => length;

        private long length;
        private DataTable keys;

        private bool invalidated;

        public DataTable() : this(8)
        {
        }

        public DataTable(int size) : this(size, size)
        {

        }

        public DataTable(string value, ScriptThread thread) : this()
        {
            for (int i = 0; i < value.Length; i++)
            {
                SetInt(thread, i, value[i]);
            }
        }

        public DataTable(IEnumerable value, ScriptThread thread) : this()
        {
            int i = 0;
            foreach (var e in value)
            {
                var kv = e as KeyValuePair<string, object>?;
                if (kv != null)
                {
                    SetString(thread, kv.Value.Key, kv.Value.Value);
                }
                else
                {
                    SetInt(thread, i++, e);
                }
            }
        }

        public DataTable(int intSize, int stringSize)
        {
            stringIndexed = new Dictionary<string, object>(stringSize + 2);
            intIndexed = new Dictionary<int, object>(intSize);
            length = 0;
            invalidated = true;

            foreach (var ot in operatorType)
            {
                operators.Add(ot.Value, null);
            }
        }

        public object GetString(ScriptThread thread, string key)
        {
            object value;
            if (specialIndices.Contains(key))
            {
                Invalidated();
                return GetSpecialString(key);
            }
            else if (stringIndexed.TryGetValue(key, out value))
                return value;
            else if (indexGetter != null /*&& !indexerLocked*/)
            {
                indexerLocked = true;
                var ret = indexGetter.Call(thread, this, new object[] { key });
                indexerLocked = false;
                return ret;
            }
            return NullClass.NullValue;
        }

        public DataTable Filter(ScriptThread thread, IEnumerable filter)
        {
            var ret = new DataTable();
            int index = 0;
            foreach (var e in filter)
            {
                try
                {
                    int i = Convert.ToInt32(e);
                    ret.SetInt(thread, index++, this.GetInt(thread, i));
                }
                catch
                {

                }
            }
            return ret;
        }

        public void FilterSet(ScriptThread thread, IEnumerable filter, object value)
        {
            foreach (var e in filter)
            {
                try
                {
                    int index = Convert.ToInt32(e);
                    SetInt(thread, index, value);
                }
                catch
                {

                }
            }
        }

        private object GetSpecialString(string key)
        {
            switch (key)
            {
                case LENGTH:
                    return length;
                case KEYS:
                    return keys;
                case GETINDEX:
                    return (object)indexGetter ?? NullClass.NullValue;
                case SETINDEX:
                    return (object)indexSetter ?? NullClass.NullValue;
                case ADD:
                case SUB:
                case MUL:
                case DIV:
                case MOD:
                case POT:
                case AND:
                case OR:
                case XOR:
                case LSH:
                case RSH:
                case UPL:
                case NEG:
                case NOT:
                case EQU:
                case NEQ:
                case GEQ:
                case LEQ:
                case LES:
                case GRE:
                    return (object)operators[operatorType[key]] ?? NullClass.NullValue;
                default:
                    return null;
            }
        }

        public void SetString(ScriptThread thread, string key, object value)
        {
            ExpressionType opType;
            if (indexSetter != null && !indexerLocked)
            {
                indexerLocked = true;
                indexSetter.Call(thread, this, new object[] { key, value });
                indexerLocked = false;
            }
            else if (key == SETINDEX)
            {
                indexSetter = value as Closure;
                if (indexSetter == null)
                    indexSetter = (value as MethodTable)?.GetIndex(new[] { TypeInfo.NotDefined, TypeInfo.NotDefined });
                return;
            }
            else if (key == GETINDEX)
            {
                indexGetter = value as Closure;
                if (indexGetter == null)
                    indexGetter = (value as MethodTable)?.GetIndex(new[] { TypeInfo.NotDefined });
                return;
            }
            else if (operatorType.TryGetValue(key, out opType))
            {
                ICallTarget closure = value as Closure;
                if (closure == null)
                    closure = unary.Contains(opType) ? (value as MethodTable)?.GetIndex(new[] { TypeInfo.NotDefined }) : (value as MethodTable)?.GetIndex(new[] { TypeInfo.NotDefined, TypeInfo.NotDefined });
                operators[opType] = closure;
                return;
            }
            else if (value == NullClass.NullValue)
            {
                stringIndexed.Remove(key);
            }
            else
            {
                stringIndexed[key] = value;
            }
            invalidated = true;
        }

        public object GetInt(ScriptThread thread, int key)
        {
            object value;
            if (intIndexed.TryGetValue(key, out value))
                return value;
            else if (indexGetter != null /*&& !indexerLocked*/)
            {
                indexerLocked = true;
                var ret = indexGetter.Call(thread, this, new object[] { key });
                indexerLocked = false;
                return ret;
            }
            return NullClass.NullValue;
        }

        public void SetInt(ScriptThread thread, int key, object value)
        {
            if (indexSetter != null && !indexerLocked)
            {
                indexerLocked = true;
                indexSetter.Call(thread, this, new object[] { key, value });
                indexerLocked = false;
            }
            else if (value == NullClass.NullValue)
            {
                intIndexed.Remove(key);
                length = intIndexed.Select(x => x.Key).Max() + 1;
            }
            else
            {
                if (length < key + 1) length = key + 1;
                intIndexed[key] = value;
            }
            invalidated = true;
        }

        private void Invalidated()
        {
            if (!invalidated) return;

            var keys = stringIndexed.Keys.ToArray();
            var dt = new DataTable(keys.Length);
            for (int i = 0; i < keys.Length; i++)
                dt.SetInt(null, i, keys[i]);

            this.keys = dt;
            invalidated = false;
        }

        public override string ToString() => "table[" + (intIndexed.Count) + "]";

        public Dictionary<string, object> GetStringIndexDict() => new Dictionary<string, object>(stringIndexed);

        public Dictionary<int, object> GetIntIndexedDict() => new Dictionary<int, object>(intIndexed);

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var e in intIndexed.OrderBy(e => e.Key))
                yield return e.Value;
        }

        public ICallTarget GetOperatorCallTarget(ExpressionType op)
        {
            ICallTarget ret;
            if (operators.TryGetValue(op, out ret))
                return ret;
            else
                return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
