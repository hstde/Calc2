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
        public TypeInfo[] ParamTypes;
        public bool HasParamsArg;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            foreach (var child in parseNode.ChildNodes)
                AddChild(AstNodeType.Parameter, "param", child);
            AsString = "param list[" + ChildNodes.Count + "]";

            var paras = new List<string>();
            var types = new List<TypeInfo>();

            for(int i = 0; i < ChildNodes.Count; i++)
            {
                var idNode = ChildNodes[i] as ParamNode;
                if (idNode != null)
                {
                    paras.Add(idNode.Ident.Symbol);
                    types.Add(idNode.IsTypeConstrained ? idNode.Type : TypeInfo.NotDefined);

                    if (i < ChildNodes.Count - 1 && idNode.IsParams)
                        context.AddMessage(Irony.ErrorLevel.Error, Location.SourceLocation, "only the last parameter may be flagged with params");
                    else if (i < ChildNodes.Count && idNode.IsParams)
                        HasParamsArg = true;
                }
                else if(ChildNodes[i] is IdentifierNode)
                {
                    var node = ChildNodes[i] as IdentifierNode;
                    paras.Add(node.Symbol);
                    types.Add(TypeInfo.NotDefined);
                }
            }
            ParamNames = paras.ToArray();
            ParamTypes = types.ToArray();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            thread.CurrentScope.ScopeInfo.AddSlot("this", SlotType.Parameter, TypeInfo.NotDefined);

            foreach (var child in ParamNames.Zip(ParamTypes, (a, b) => new { name = a, type = b }))
            {
                thread.CurrentScope.ScopeInfo.AddSlot(child.name, SlotType.Parameter, child.type);
            }

            Evaluate = (t) => t.Runtime.NullValue;

            thread.CurrentNode = Parent;
            return thread.Runtime.NullValue;
        }
    }
}
