using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class ArrayDefNode : AstNode
    {
        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            foreach (var child in parseNode.GetMappedChildNodes())
                if (child != null)
                    AddChild("ArrayItem", child);
            AsString = "Array[" + ChildNodes.Count + "]";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            DataTable result = new DataTable(ChildNodes.Count);
            int indexCounter = 0;

            foreach (AstNode t in ChildNodes)
            {
                object tmp = t.Evaluate(thread);
                object[] arr = tmp as object[];
                if(arr != null)
                {
                    if (arr.Length != 2)
                        thread.ThrowScriptError("Failed to create array.");

                    string name = (string)arr[0];

                    var iCall = arr[1] as ICallTarget;

                    var mTable = result.GetString(name) as MethodTable;

                    if(iCall != null)
                    {
                        if(mTable != null)
                        {
                            mTable.Add(iCall);
                            arr[1] = mTable;
                        }
                        else
                        {
                            mTable = new MethodTable(name);
                            mTable.Add(iCall);
                            arr[1] = mTable;
                        }
                    }

                    result.SetString(name, arr[1]);
                }
                else
                {
                    result.SetInt(indexCounter, tmp);
                    indexCounter++;
                }
            }

            thread.CurrentNode = Parent;
            return result;
        }
    }

    public class NamedArrayItemNode : AstNode
    {
        private string name;
        private AstNode value;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            name = nodes[0].FindTokenAndGetText();
            if (name.StartsWith("\"")) name = name.Remove(name.Length - 1, 1).Remove(0, 1);
            value = AddChild("Value", nodes[2]);

            AsString = "[" + name + "] = " + value.AsString;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            object[] result = new object[2];
            result[0] = name;
            result[1] = value.Evaluate(thread);

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
