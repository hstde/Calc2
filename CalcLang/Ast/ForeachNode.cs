﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class ForeachNode : AstNode
    {
        public AstNode IterVarBlock;
        public AstNode InExpr;
        public AstNode Block;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            IterVarBlock = AddChild("IterVar", nodes[0]);
            InExpr = AddChild("InExpr", nodes[1]);
            Block = AddChild("ForeachBlock", nodes[2]);

            AsString = "Forech Loop";
        }

        public override void Reset()
        {
            DependentScopeInfo = null;
            base.Reset();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object result = thread.Runtime.NullValue;
            FlowControl = FlowControl.Next;

            if (DependentScopeInfo == null)
                DependentScopeInfo = new ScopeInfo(this, Parent?.AsString ?? "");

            thread.PushScope(DependentScopeInfo, null);

            IterVarBlock?.Evaluate(thread);
            DataTable foreachObject = null;
            var rawobject = InExpr.Evaluate(thread);
            if (rawobject is string)
            {
                foreachObject = new DataTable((string)rawobject);
            }
            else if (rawobject is DataTable)
            {
                foreachObject = (DataTable)rawobject;
            }
            else if(rawobject is IEnumerable)
            {
                foreachObject = new DataTable((IEnumerable)rawobject);
            }
            else
            {
                thread.ThrowScriptError("Can't iterate over object of type {0}", rawobject.GetType().Name);
            }

            foreach (var e in foreachObject)
            {
                IterVarBlock.SetValue(thread, e);
                Block.Evaluate(thread);

                if (FlowControl == FlowControl.Break)
                    break;

                if (FlowControl == FlowControl.Continue)
                    FlowControl = FlowControl.Next;

                if (FlowControl == FlowControl.Return)
                    break;
            }
            thread.PopScope();

            if (FlowControl == FlowControl.Return && Parent != null)
                Parent.FlowControl = FlowControl.Return;

            thread.CurrentNode = Parent;
            return result;
        }
    }
}