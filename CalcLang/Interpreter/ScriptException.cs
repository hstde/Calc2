using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace CalcLang.Interpreter
{
    [Serializable]
    public class ScriptException : Exception
    {
        public SourceInfo Location;
        public ScriptStackTrace ScriptStackTrace;

        public ScriptException(string message) : this(message, null)
        {
        }

        public ScriptException(string message, Exception inner) : this(message, inner, SourceLocation.Empty, null)
        {
        }

        public ScriptException(string message, Exception inner, SourceInfo location, ScriptStackTrace stack) : base(message, inner)
        {
            Location = location;
            ScriptStackTrace = stack;
        }

        public override string ToString() => Message
            + Environment.NewLine + ScriptStackTrace.ToString();

        public object ToScriptObject()
            => Message;
    }
}
