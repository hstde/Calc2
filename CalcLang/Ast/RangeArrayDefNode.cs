﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using Irony.Ast;
using Irony.Parsing;

namespace CalcLang.Ast
{
    public class RangeArrayDefNode : AstNode
    {
        public AstNode from;
        public AstNode to;
        public AstNode step;
        public bool isInclusive;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            var nodes = parseNode.GetMappedChildNodes();
            from = AddChild("from", nodes[0]);
            isInclusive = nodes[1].FindTokenAndGetText().Length == 3;
            to = AddChild("to", nodes[2]);
            step = nodes[3].ChildNodes.Count > 0 ? AddChild("step", nodes[3].ChildNodes[0]) : null;

            AsString = "Range " + from + " " + to + (isInclusive? " (inclusive)" : "") + (step != null ? " " + step : "");
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;

            var from = (long)this.from.Evaluate(thread);
            var to = (long)this.to.Evaluate(thread);
            var step = this.step != null ? (long)this.step.Evaluate(thread) : 1;


            IEnumerable result;
            if (this.step == null)
                result = new Range(from, to, isInclusive);
            else
                result = new RangeWithStep(from, to, step, isInclusive);

            thread.CurrentNode = Parent;

            return result;
        }
    }
}
