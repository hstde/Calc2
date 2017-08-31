using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private readonly List<string> specialIndicies = new List<string> { KEYS, LENGTH, GETINDEX, SETINDEX };

        private Dictionary<string, object> stringIndexed;
        private readonly Dictionary<int, object> intIndexed;

        private bool indexerLocked = false;
        private ICallTarget indexGetter = null;
        private ICallTarget indexSetter = null;

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
            else if (specialIndicies.Contains(key))
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
                default:
                    return null;
            }
        }

        public void SetString(ScriptThread thread, string key, object value)
        {
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
                    indexSetter = (value as MethodTable).GetIndex(2);
                return;
            }
            else if(key == GETINDEX)
            {
                indexGetter = value as Closure;
                if (indexGetter == null)
                    indexGetter = (value as MethodTable).GetIndex(1);
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
            if(indexSetter != null && !indexerLocked)
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
            foreach (var e in intIndexed.OrderBy(e=>e.Key))
                yield return e.Value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
