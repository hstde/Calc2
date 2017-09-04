﻿using System;
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
        private const string KEYS = "Keys";
        private const string LENGTH = "Length";
        private const string GETINDEX = "__getIndex";
        private const string SETINDEX = "__setIndex";
        private const string ADD = "__add";
        private const string SUB = "__sub";
        private const string MUL = "__mul";
        private const string DIV = "__div";
        private const string MOD = "__mod";
        private const string AND = "__and";
        private const string OR = "__or";
        private const string XOR = "__xor";
        private const string LSH = "__lsh";
        private const string RSH = "__rsh";
        private const string UPL = "__upl";
        private const string NEG = "__neg";
        private const string NOT = "__not";
        private const string EQU = "__equ";
        private const string NEQ = "__neq";
        private const string GEQ = "__geq";
        private const string LEQ = "__leq";
        private const string LES = "__les";
        private const string GRE = "__gre";

        private readonly List<string> specialIndices = new List<string> { KEYS, LENGTH, GETINDEX, SETINDEX,
            ADD, SUB, MUL, DIV, MOD, AND, OR, XOR, LSH, RSH, UPL, NEG, NOT, EQU, NEQ, GEQ, LEQ, LES, GRE };
        private readonly List<string> operatorIndices = new List<string> { ADD, SUB, MUL, DIV, MOD, AND, OR, XOR, LSH, RSH, UPL, NEG, NOT, EQU, NEQ, GEQ, LEQ, LES, GRE };
        private readonly List<ExpressionType> unary = new List<ExpressionType> { ExpressionType.UnaryPlus, ExpressionType.Negate, ExpressionType.Not };

        private Dictionary<string, object> stringIndexed;
        private readonly Dictionary<int, object> intIndexed;

        private bool indexerLocked = false;
        private ICallTarget indexGetter = null;
        private ICallTarget indexSetter = null;

        private Dictionary<ExpressionType, ICallTarget> operators = new Dictionary<ExpressionType, ICallTarget>
        {
            [ExpressionType.AddChecked] = null
        };
        private Dictionary<string, ExpressionType> operatorType = new Dictionary<string, ExpressionType>
        {
            [ADD] = ExpressionType.AddChecked,
            [SUB] = ExpressionType.SubtractChecked,
            [MUL] = ExpressionType.MultiplyChecked,
            [DIV] = ExpressionType.Divide,
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

        public decimal Length => length;

        private decimal length;
        private DataTable keys;

        private bool invalidated;

        public DataTable() : this(8) { }

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
        }

        public object GetString(ScriptThread thread, string key)
        {
            object value;
            if (indexGetter != null && !indexerLocked)
            {
                indexerLocked = true;
                var ret = indexGetter.Call(thread, this, new object[] { key });
                indexerLocked = false;
                return ret;
            }
            else if (specialIndices.Contains(key))
            {
                Invalidated();
                return GetSpecialString(key);
            }
            else if (stringIndexed.TryGetValue(key, out value))
                return value;
            return NullClass.NullValue;
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
                    return (object)operators[operatorType[key]] == NullClass.NullValue;
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
                    indexSetter = (value as MethodTable)?.GetIndex(2);
                return;
            }
            else if (key == GETINDEX)
            {
                indexGetter = value as Closure;
                if (indexGetter == null)
                    indexGetter = (value as MethodTable)?.GetIndex(1);
                return;
            }
            else if (operatorType.TryGetValue(key, out opType))
            {
                ICallTarget closure = value as Closure;
                if (closure == null)
                    closure = unary.Contains(opType) ? (value as MethodTable)?.GetIndex(1) : (value as MethodTable)?.GetIndex(2);
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
            if (indexGetter != null && !indexerLocked)
            {
                indexerLocked = true;
                var ret = indexGetter.Call(thread, this, new object[] { key });
                indexerLocked = false;
                return ret;
            }
            else if (intIndexed.TryGetValue(key, out value))
                return value;
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
