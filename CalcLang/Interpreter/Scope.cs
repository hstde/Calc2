using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class Scope
    {
        public ScopeInfo ScopeInfo;
        public volatile object[] Values;

        public object[] Parameters;
        public Scope Caller;
        public Scope Creator;
        private Scope parent;

        public Scope Parent
        {
            get
            {
                if (parent == null)
                    parent = GetParent();
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        public Scope(ScopeInfo scopeInfo, Scope caller, Scope creator, object[] parameters)
        {
            ScopeInfo = scopeInfo;
            Caller = caller;
            Creator = creator;
            Parameters = parameters;
            Values = new object[scopeInfo.ValuesCount == 0 ? 8 : scopeInfo.ValuesCount];
        }

        public SlotInfo AddSlot(string name, TypeInfo type)
        {
            var slot = ScopeInfo.AddSlot(name, SlotType.Value, type);
            if (slot.Index >= Values.Length)
                Resize(Values.Length * 2);
            return slot;
        }

        public object[] GetValues() => Values;

        public object GetValue(int index)
        {
            try
            {
                var tmp = Values;
                return tmp[index];
            }
            catch (NullReferenceException)
            {
                System.Threading.Thread.Sleep(0);
                return GetValue(index);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        public void SetValue(int index, object value)
        {
            try
            {
                var tmp = Values;
                tmp[index] = value;
                if (tmp != Values)
                    SetValue(index, value);
            }
            catch (NullReferenceException)
            {
                System.Threading.Thread.Sleep(0);
                SetValue(index, value);
            }
            catch (IndexOutOfRangeException)
            {
                Resize(ScopeInfo.GetSlotCount());
                SetValue(index, value);
            }
        }

        protected Scope GetParent()
        {
            var parentScopeInfo = ScopeInfo.Parent;
            if (parentScopeInfo == null)
                return null;
            var current = Creator;
            while (current != null)
            {
                if (current.ScopeInfo == parentScopeInfo)
                    return current;
                current = current.Creator;
            }
            return null;
        }

        protected void Resize(int newSize)
        {
            lock (ScopeInfo.LockObject)
            {
                if (Values.Length >= newSize) return;
                object[] tmp = System.Threading.Interlocked.Exchange(ref Values, null);
                Array.Resize(ref tmp, newSize);
                System.Threading.Interlocked.Exchange(ref Values, tmp);
            }
        }

        public IDictionary<string, object> AsDictionary() => new ScopeValuesDictionary(this);

        public override string ToString() => ScopeInfo.ToString();
    }
}
