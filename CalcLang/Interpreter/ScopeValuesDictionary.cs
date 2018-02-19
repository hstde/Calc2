using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class ScopeValuesDictionary : IDictionary<string, object>
    {
        private Scope scope;

        internal ScopeValuesDictionary(Scope scope)
        {
            this.scope = scope;
        }

        public object this[string key]
        {
            get
            {
                object value;
                TryGetValue(key, out value);
                return value;
            }

            set
            {
                Add(key, value);
            }
        }

        public int Count => scope.ScopeInfo.GetSlotCount();

        public bool IsReadOnly => true;

        public ICollection<string> Keys => scope.ScopeInfo.GetNames();

        public ICollection<object> Values => scope.GetValues();

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            Add(key, TypeInfo.NotDefined, value);
        }

        public void Add(string key, TypeInfo type, object value)
        {
            var slot = scope.ScopeInfo.GetSlot(key);
            if (slot == null)
                slot = scope.AddSlot(key, type);
            scope.SetValue(slot.Index, value);
        }

        public void Clear()
        {
            var values = scope.GetValues();
            for (int i = 0; i < values.Length; i++)
                values[i] = null;
        }

        public bool Contains(KeyValuePair<string, object> item) => scope.ScopeInfo.GetSlot(item.Key) != null;

        public bool ContainsKey(string key) => scope.ScopeInfo.GetSlot(key) != null;

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            var slots = scope.ScopeInfo.GetSlots();
            foreach (var slot in slots)
                yield return new KeyValuePair<string, object>(slot.Name, scope.GetValue(slot.Index));
        }

        public bool Remove(KeyValuePair<string, object> item) => Remove(item.Key);

        public bool Remove(string key)
        {
            this[key] = null;
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;
            SlotInfo slot = scope.ScopeInfo.GetSlot(key);
            if (slot == null)
                return false;
            value = scope.GetValue(slot.Index);
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
