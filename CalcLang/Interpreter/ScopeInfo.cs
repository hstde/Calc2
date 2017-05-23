using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class ScopeInfoList : List<ScopeInfo> { }
    public class ScopeInfo
    {
        public int ValuesCount, ParametersCount;
        private ScopeInfo parent;

        public Ast.AstNode OwnerNode;
        public int StaticIndex = -1;
        public int Level;
        public string AsString => Name + " (" + (OwnerNode == null ? "Level=" + Level : OwnerNode.ToString() + ", Level=" + Level) + ")";

        public string Name
        {
            get;
            set;
        }
        private SlotInfoDictionary slots;


        internal protected object LockObject = new object();

        public ScopeInfo Parent
        {
            get
            {
                if (parent == null)
                    parent = GetParent();
                return parent;
            }
        }

        public ScopeInfo(Ast.AstNode ownerNode, string name = "")
        {
            Util.Check(ownerNode != null, "ScopeInfo owner node must not be null.");

            OwnerNode = ownerNode;
            Level = Parent == null ? 0 : Parent.Level + 1;
            slots = new SlotInfoDictionary();
            Name = name;
        }


        public ScopeInfo GetParent()
        {
            if (OwnerNode == null) return null;
            var current = OwnerNode.Parent;
            while (current != null)
            {
                var result = current.DependentScopeInfo;
                if (result != null) return result;
                current = current.Parent;
            }
            return null;
        }

        public SlotInfo AddSlot(string name, SlotType type)
        {
            lock (LockObject)
            {
                var index = type == SlotType.Value ? ValuesCount++ : ParametersCount++;
                var slot = new SlotInfo(this, type, name, index);
                slots.Add(name, slot);
                return slot;
            }
        }

        public SlotInfo GetSlot(string name)
        {
            lock (LockObject)
            {
                SlotInfo slot;
                slots.TryGetValue(name, out slot);
                return slot;
            }
        }

        public IList<SlotInfo> GetSlots()
        {
            lock (LockObject)
                return new List<SlotInfo>(slots.Values);
        }
        public IList<string> GetNames()
        {
            lock (LockObject)
                return new List<string>(slots.Keys);
        }

        public int GetSlotCount()
        {
            lock (LockObject)
                return slots.Count;
        }

        public override string ToString() => AsString;
    }
}
