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
    public class ParamListNode : AstNode
    {
        public string[] ParamNames;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            foreach (var child in parseNode.ChildNodes)
                AddChild(AstNodeType.Parameter, "param", child);
            AsString = "param list[" + ChildNodes.Count + "]";

            var paras = new List<string>();
            foreach (var child in ChildNodes)
            {
                var idNode = child as IdentifierNode;
                if (idNode != null)
                {
                    paras.Add(idNode.Symbol);
                }
            }
            ParamNames = paras.ToArray();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            thread.CurrentScope.ScopeInfo.AddSlot("this", SlotType.Parameter);

            foreach (var child in ParamNames)
            {
                thread.CurrentScope.ScopeInfo.AddSlot(child, SlotType.Parameter);
            }

            Evaluate = (t) => t.Runtime.NullValue;

            thread.CurrentNode = Parent;
            return thread.Runtime.NullValue;
        }
    }
}
