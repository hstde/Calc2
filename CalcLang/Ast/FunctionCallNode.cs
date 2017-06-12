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
    public class FunctionCallNode : AstNode
    {
        private AstNode TargetRef;
        private AstNode Arguments;
        private string targetName;

        public override void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);
            var nodes = parseNode.GetMappedChildNodes();
            TargetRef = AddChild("Target", nodes[0]);
            targetName = TargetRef.AsString;
            Arguments = AddChild("Args", nodes[1]);
            AsString = "Call " + targetName;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;
            object result = thread.Runtime.NullValue;

            var target = TargetRef.Evaluate(thread);

            int argCount = Arguments.ChildNodes.Count;

            var iCall = target as ICallTarget;
            var mTable = target as MethodTable;

            if (mTable != null)
            {
                iCall = mTable.GetIndex(argCount);
                if (iCall == null)
                    thread.ThrowScriptError("There is no function with name {0}, that takes {1} arguments.", targetName, argCount);
            }

            if (iCall == null)
                thread.ThrowScriptError("Variable {0} of type {1} is not a callable function.", targetName, target.GetType().Name);

            if(iCall.GetParameterCount() != argCount && iCall.GetParameterCount() != -1 && !iCall.GetHasParamsArg())
                thread.ThrowScriptError("{0}() does not take {1} arguments.", targetName, argCount);

            var args = (object[])Arguments.Evaluate(thread);

            if(iCall.GetHasParamsArg() && (iCall.GetParameterCount() != argCount || !(args[args.Length - 1] is DataTable)))
            {
                var tempArgs = new object[iCall.GetParameterCount()];
                Array.Copy(args, tempArgs, tempArgs.Length - 1);
                int lastIndex = tempArgs.Length - 1;
                var tempDT = new DataTable(args.Length - lastIndex);
                for (int i = lastIndex; i < args.Length; i++)
                    tempDT.SetInt(i - lastIndex, args[i]);
                tempArgs[lastIndex] = tempDT;
                args = tempArgs;
            }

            var fi = iCall.GetFunctionInfo();

            PushCallStack(thread);
            var back = thread.CurrentFuncInfo;
            thread.CurrentFuncInfo = fi;

            object thisRef = thread.Runtime.NullValue;

            if(TargetRef is MemberAccessNode)
            {
                var man = (MemberAccessNode) TargetRef;
                thisRef = man.lastTargetValue;
            }
            else
            {
                var bind = thread.Bind("this", BindingRequestFlags.Existing | BindingRequestFlags.NullOk | BindingRequestFlags.Read);
                if (bind != null && !(bind is NullBinding) && bind.GetValueRef != null)
                    thisRef = bind.GetValueRef(thread);
            }

            result = iCall.Call(thread, thisRef, args);
            thread.CallStack.Pop();
            thread.CurrentFuncInfo = back;

            thread.CurrentNode = Parent;
            return result;
        }

        private void PushCallStack(ScriptThread thread)
        {
            thread.CurrentFuncInfo.CallLocation = Location;
            thread.CallStack.Push(thread.CurrentFuncInfo);
        }
    }
}
