using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using System.Reflection;
using CalcLang.Interpreter;

namespace CalcLang.Ast
{
    public class IndexedAccessNode : AstNode
    {
        AstNode target, index;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();

            target = AddChild("Target", nodes[0]);
            index = AddChild("Index", nodes[1]);
            AsString = target + "[" + index + "]";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object result = thread.Runtime.NullValue;

            var targetValue = target.Evaluate(thread);
            if (targetValue == null)
                thread.ThrowScriptError("Target object is null.");

            var type = targetValue.GetType();
            var indexValue = index.Evaluate(thread);

            if (type == typeof(string))
            {
                var sTarget = targetValue as string;
                var iIndex = Convert.ToInt32(indexValue);

                if (iIndex < sTarget.Length)
                    result = sTarget[iIndex];
                else
                    result = '\0';
            }
            else if (type == typeof(DataTable))
            {
                try
                {
                    var dtTarget = targetValue as DataTable;
                    string sIndex = indexValue as string;
                    var dtIndex = indexValue as DataTable;

                    if (sIndex != null)
                    {
                        result = dtTarget.GetString(thread, sIndex);
                    }
                    else if(dtIndex != null)
                    {
                        result = dtTarget.Filter(thread, dtIndex);
                    }
                    else
                    {
                        int iIndex = Convert.ToInt32(indexValue);
                        result = dtTarget.GetInt(thread, iIndex);
                    }
                }
                catch(Exception)
                {
                    thread.ThrowScriptError("Index must be string, number or table.");
                }
            }
            else if (type.IsArray)
            {
                var arr = targetValue as Array;
                var iIndex = Convert.ToInt32(indexValue);
                result = arr.GetValue(iIndex);
            }
            else if (targetValue is System.Collections.IDictionary)
            {
                var dict = (System.Collections.IDictionary)targetValue;
                result = dict[indexValue];
            }
            else
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.InvokeMethod;
                result = type.InvokeMember("get_Item", flags, null, targetValue, new object[] { indexValue });
            }

            thread.CurrentNode = Parent;
            return result;
        }

        public override void DoSetValue(ScriptThread thread, object value)
        {
            thread.CurrentNode = this;

            var targetValue = target.Evaluate(thread);
            if (targetValue == null)
                thread.ThrowScriptError("Target object is null");

            if (targetValue == thread.Runtime.NullValue)
                thread.ThrowScriptError("NullReferenceException. target was null (" + target.AsString + ")");

            var type = targetValue.GetType();
            var indexValue = index.Evaluate(thread);

            if (type == typeof(string))
            {
                thread.ThrowScriptError("String is read-only");
            }
            else if (type == typeof(DataTable))
            {
                try
                {
                    var dtTarget = targetValue as DataTable;
                    string sIndex = indexValue as string;

                    if (sIndex != null)
                    {
                        dtTarget.SetString(thread, sIndex, value);
                    }
                    else
                    {
                        int iIndex = Convert.ToInt32(indexValue);
                        dtTarget.SetInt(thread, iIndex, value);
                    }
                }
                catch(OutOfMemoryException e)
                {
                    thread.ThrowScriptError("Out of Memory exception!");
                }
                catch(Exception e)
                {
                    thread.ThrowScriptError("Index must be string or number. Exception was " + e.GetType() + " " + e.Message);
                }
            }
            else if (type.IsArray)
            {
                var arr = targetValue as Array;
                var iIndex = (int)Convert.ToDecimal(indexValue);
                arr.SetValue(value, iIndex);
            }
            else if (targetValue is System.Collections.IDictionary)
            {
                var dict = (System.Collections.IDictionary)targetValue;
                dict[indexValue] = value;
            }
            else
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.InvokeMethod;
                type.InvokeMember("set_Item", flags, null, targetValue, new object[] { indexValue, value });
            }

            thread.CurrentNode = Parent;
        }
    }
}
