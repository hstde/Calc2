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
        public bool HasParamsArg;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            foreach (var child in parseNode.ChildNodes)
                AddChild(AstNodeType.Parameter, "param", child);
            AsString = "param list[" + ChildNodes.Count + "]";

            var paras = new List<string>();
            for(int i = 0; i < ChildNodes.Count; i++)
            {
                var idNode = ChildNodes[i] as ParamNode;
                if (idNode != null)
                {
                    paras.Add(idNode.Ident.Symbol);
                    if (i < ChildNodes.Count - 1 && idNode.IsParams)
                        context.AddMessage(Irony.ErrorLevel.Error, Location.SourceLocation, "only the last parameter may be flagged with params");
                    else if (i < ChildNodes.Count && idNode.IsParams)
                        HasParamsArg = true;
                }
                else if(ChildNodes[i] is IdentifierNode)
                {
                    var node = ChildNodes[i] as IdentifierNode;
                    paras.Add(node.Symbol);
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
