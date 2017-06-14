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
        private Dictionary<string, object> stringIndexed;
        private readonly Dictionary<int, object> intIndexed;

        private decimal length;
        private decimal count;
        private DataTable keys;

        private bool invalidated;

        public DataTable() : this(8) { }

        public DataTable(int size) : this(size, size)
        {

        }

        public DataTable(string value) : this()
        {
            for (int i = 0; i < value.Length; i++)
            {
                SetInt(i, value[i]);
            }
        }

        public DataTable(IEnumerable value) : this()
        {
            int i = 0;
            foreach (var e in value)
            {
                var kv = e as KeyValuePair<string, object>?;
                if (kv != null)
                {
                    SetString(kv.Value.Key, kv.Value.Value);
                }
                else
                {
                    SetInt(i++, e);
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

        public object GetString(string key)
        {
            if (key == "Count" || key == "Length" || key == "Keys")
            {
                Invalidated();
                return GetSpecialString(key);
            }

            object value;
            if (stringIndexed.TryGetValue(key, out value))
                return value;
            return NullClass.NullValue;
        }

        private object GetSpecialString(string key)
        {
            switch (key)
            {
                case "Count":
                    return count;
                case "Length":
                    return length;
                case "Keys":
                    return keys;
                default:
                    return null;
            }
        }

        public void SetString(string key, object value)
        {
            if (value == NullClass.NullValue)
            {
                stringIndexed.Remove(key);
            }
            else
            {
                stringIndexed[key] = value;
            }
            invalidated = true;
        }

        public object GetInt(int key)
        {
            object value;
            if (intIndexed.TryGetValue(key, out value))
                return value;
            return NullClass.NullValue;
        }

        public void SetInt(int key, object value)
        {
            if (value == NullClass.NullValue)
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

            count = stringIndexed.Count + intIndexed.Count;

            var keys = stringIndexed.Keys.ToArray();
            var dt = new DataTable(keys.Length);
            for (int i = 0; i < keys.Length; i++)
                dt.SetInt(i, keys[i]);

            this.keys = dt;
            invalidated = false;
        }

        public override string ToString() => "table[" + (stringIndexed.Count + intIndexed.Count) + "]";

        public Dictionary<string, object> GetStringIndexDict() => new Dictionary<string, object>(stringIndexed);

        public Dictionary<int, object> GetIntIndexedDict() => new Dictionary<int, object>(intIndexed);

        public IEnumerator<object> GetEnumerator()
        {
            foreach (var e in intIndexed)
                yield return new DataTable() { stringIndexed = new Dictionary<string, object> { ["Key"] = e.Key, ["Value"] = e.Value } };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
