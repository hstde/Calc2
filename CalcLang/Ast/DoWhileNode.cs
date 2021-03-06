﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class DoWhileNode : AstNode
    {
        public AstNode Condition;
        public AstNode Body;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            Condition = AddChild("Condition", nodes[1]);
            Body = AddChild("Body", nodes[0]);

            AsString = "Do While Loop";
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            FlowControl = FlowControl.Next;
            object result = thread.Runtime.NullValue;
            bool cond = false;

            do
            {
                result = Body.Evaluate(thread);

                if (Body.FlowControl == FlowControl.Break)
                    break;

                if (Body.FlowControl == FlowControl.Continue)
                    FlowControl = FlowControl.Next;

                if (Body.FlowControl == FlowControl.Return)
                    break;

                cond = thread.Runtime.IsTrue(Condition.Evaluate(thread));

            } while (cond);

            if (Body.FlowControl == FlowControl.Return)
                FlowControl = FlowControl.Return;

            thread.CurrentNode = Parent;
            return result;
        }
    }
}
