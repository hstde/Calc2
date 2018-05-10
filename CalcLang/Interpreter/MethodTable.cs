using System;
using System.Collections.Generic;
using System.Linq;

namespace CalcLang.Interpreter
{
    public class MethodTable
    {
        private Dictionary<int, List<ICallTarget>> elements;
        public readonly string Name;

        public MethodTable(string name)
        {
            elements = new Dictionary<int, List<ICallTarget>>();
            Name = name;
        }

        public void Add(ICallTarget value)
        {
            //elements.Add(key, value);
            value.MethodTable = this;
            if (!elements.TryGetValue(value.GetParameterCount(), out List<ICallTarget> res))
            {
                res = new List<ICallTarget>();
                elements.Add(value.GetParameterCount(), res);
            }
            res.Add(value);
        }

        public Binding Bind(BindingRequest request)
        {
            throw new NotImplementedException();
        }

        public ICallTarget GetIndex(TypeInfo[] types)
        {
            if (types == null)
                types = new TypeInfo[0];
            int key = types.Length;

            if (elements.TryGetValue(key, out List<ICallTarget> res))
            {
            }
            else if (elements.TryGetValue(-1, out res))
            {
                //if we have a method that takes varargs, than hey, return that
            }
            //now we have to check every method if the last arg is a params arg
            else
            {
                foreach (var e in elements.Where(x => x.Key - 1 <= key).OrderByDescending(x => x.Key)) // take the method with the fewest params first; why the Key - 1, because params could be empty
                {
                    res = e.Value.Where(x => x.GetFunctionInfo().HasParamsArg).ToList();
                    if (res.Any())
                        break;
                }
            }

            if (res != null)
            {
                //find the best fit
                foreach (var f in res)
                {
                    var functionTypes = f.GetFunctionInfo().ParamTypes;
                    if (functionTypes == null)
                        functionTypes = new TypeInfo[0];

                    var fit = functionTypes.Zip(types.Take(functionTypes.Length), (a, b) => new { a, b }).Select(e => Runtime.IsTypeMatch(e.a, e.b));
                    if (fit.All(e => e))
                        return f;
                }
            }

            return null;
        }

        public IEnumerable<ICallTarget> AsEnumerable()
        {
            foreach (var list in elements.Where(e => e.Value != null))
                foreach (var e in list.Value.Where(e => e != null))
                    yield return e;
        }

        public override string ToString() => "Function";
    }
}
