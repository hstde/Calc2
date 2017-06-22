using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Ast;

namespace CalcLang.Interpreter
{
    [Flags]
    public enum BindingRequestFlags
    {
        Read = 1 << 1,
        Write = 1 << 2,
        Invoke = 1 << 3,
        Existing = 1 << 4,
        NullOk = 1 << 5,
        PreferExisting = 1 << 6,
        NewOnly = 1 << 7,
        Extern = 1 << 8,

        ExistingOrNew = Existing | NewOnly
    }

    public class BindingRequest
    {
        public ScriptThread Thread;
        public AstNode FromNode;
        public BindingRequestFlags Flags;
        public string Symbol;
        public ScopeInfo FromScopeInfo;

        public BindingRequest(ScriptThread thread, AstNode fromNode, string symbol, BindingRequestFlags flags)
        {
            Thread = thread;
            FromNode = fromNode;
            Symbol = symbol;
            Flags = flags;
            FromScopeInfo = thread.CurrentScope.ScopeInfo;
        }
    }
}
