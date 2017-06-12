using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class MethodTable
    {
        private Dictionary<int, ICallTarget> elements;
        public readonly string Name;

        public MethodTable(string name)
        {
            elements = new Dictionary<int, ICallTarget>();
            Name = name;
        }

        public void Add(ICallTarget value)
        {
            //elements.Add(key, value);
            value.MethodTable = this;
            elements[value.GetParameterCount()] = value;
        }

        public Binding Bind(BindingRequest request)
        {
            throw new NotImplementedException();
        }

        public ICallTarget GetIndex(int key)
        {
            ICallTarget res;
            if (elements.TryGetValue(key, out res))
            {
                return res;
            }
            if (elements.TryGetValue(-1, out res))
            {
                //if we have a method that takes varargs, than hey, return that
                return res;
            }
            //now we have to check every method if the last arg is a params arg

            foreach (var e in elements.Where(x => x.Key - 1 <= key).OrderByDescending(x => x.Key)) // take the method with the fewest params first; why the Key - 1, because params could be empty
            {
                if (e.Value.GetFunctionInfo().HasParamsArg)
                    return e.Value;
            }

            return null;
        }

        public override string ToString() => "Function";
    }
}
