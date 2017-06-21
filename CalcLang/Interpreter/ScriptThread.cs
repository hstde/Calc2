using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Ast;
using Irony.Parsing;

namespace CalcLang.Interpreter
{
    public class ScriptThread : IBindingSource
    {
        public readonly ScriptApp App;
        public readonly Runtime Runtime;
        public Stack<FunctionInfo> CallStack;

        public AstNode CurrentNode { get; set; }
        public Scope CurrentScope { get; set; }

        public Scope RootScope => App.MainScope;

        public FunctionInfo CurrentFuncInfo { get; set; }

        public ScriptThread(ScriptApp app)
        {
            App = app;
            Runtime = App.Runtime;
            CurrentScope = App.MainScope;
            CallStack = new Stack<FunctionInfo>();
        }

        public void PushScope(ScopeInfo scopeInfo, object[] parameters)
        {
            CurrentScope = new Scope(scopeInfo, CurrentScope, CurrentScope, parameters);
        }

        public void PushClosureScope(ScopeInfo scopeInfo, Scope closureParent, object[] parameters)
        {
            CurrentScope = new Scope(scopeInfo, CurrentScope, closureParent, parameters);
        }

        public void PopScope()
        {
            CurrentScope = CurrentScope.Caller;
        }

        public Binding Bind(string symbol, BindingRequestFlags options)
        {
            var request = new BindingRequest(this, CurrentNode, symbol, options);
            var binding = Bind(request);
            if (binding == null)
                ThrowScriptError("Unknown symbol '{0}'.", symbol);
            return binding;
        }

        public object HandleError(Exception exception)
        {
            if (exception is ScriptException)
                throw exception;
            var stack = GetStackTrace(true);
            throw new ScriptException(exception.Message, exception, CurrentNode.ErrorAnchor, stack);
        }

        public void ThrowScriptError(string message, params object[] args)
        {
            ThrowScriptError(message, true, args);
        }

        public void ThrowScriptError(string message, bool withHere, object[] args)
        {
            if(args?.Length > 0)
            {
                message = string.Format(message, args);
            }
            var loc = GetCurrentLocation();
            var stack = GetStackTrace(withHere);
            throw new ScriptException(message, null, loc, stack);
        }

        public void ThrowScriptException(object payload)
        {
            var loc = GetCurrentLocation();
            var stack = GetStackTrace(true);
            throw ScriptException.CreateScriptException(payload, loc, stack);
        }

        private SourceInfo GetCurrentLocation() => CurrentNode == null ? new SourceLocation() : CurrentNode.Location;
        public ScriptStackTrace GetStackTrace(bool withHere)
        {
            CurrentFuncInfo.CallLocation = GetCurrentLocation();
            if (withHere)
            {
                CallStack.Push(CurrentFuncInfo);
            }

            var ret = new ScriptStackTrace(CallStack);

            if (withHere)
            {
                CallStack.Pop();
            }
            return ret;
        }

        public Binding Bind(BindingRequest request) => Runtime.Bind(request);
    }
}
