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
        public object Payload;

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

        public static ScriptException CreateScriptException(object payload, SourceInfo location, ScriptStackTrace stack)
            => new ScriptException(payload as string ?? "ScriptException", null, location, stack) { Payload = payload };

        public override string ToString()
        {
            var msg = Message;
            if (Payload is DataTable)
            {
                var dt = (DataTable)Payload;
                var exType = dt.GetString(null, "ExceptionType").ToString();
                msg = dt.GetString(null, "Message").ToString();
                if (msg != null && msg.Length > 0)
                    msg = exType + ": " + msg;
                else
                    msg = exType;
            }
            return msg + Environment.NewLine + ScriptStackTrace.ToString();
        }

        public object ToScriptObject()
        {
            var ret = new DataTable();
            if (Payload is DataTable)
            {
                foreach (var e in ((DataTable)Payload).GetStringIndexDict())
                {
                    ret.SetString(null, e.Key, e.Value);
                }
            }
            else
            {
                ret.SetString(null, "Message", Payload);
            }

            var dataStackTrace = new DataTable();
            int index = 0;
            foreach (var e in ScriptStackTrace.CallStack)
            {
                var stackEntry = new DataTable();
                stackEntry.SetString(null, "Name", e.Name);
                stackEntry.SetString(null, "ParamCount", e.ParamCount);
                if (e.ParamNames != null)
                    stackEntry.SetString(null, "ParamNames", new DataTable(e.ParamNames, null));
                else
                    stackEntry.SetString(null, "ParamNames", new DataTable());
                stackEntry.SetString(null, "CallLocation", new DataTable(
                    new Dictionary<string, object>()
                    {
                        ["Filename"] = e.CallLocation.FileName,
                        ["Column"] = e.CallLocation.SourceLocation.Column,
                        ["Line"] = e.CallLocation.SourceLocation.Line,
                        ["Position"] = e.CallLocation.SourceLocation.Position
                    }, null));
                stackEntry.SetString(null, "HasParamsArg", e.HasParamsArg);

                dataStackTrace.SetInt(null, index++, stackEntry);
            }

            ret.SetString(null, "Stacktrace", dataStackTrace);

            ret.SetString(null, "Location", new DataTable(
                new Dictionary<string, object>()
                {
                    ["Filename"] = Location.FileName,
                    ["Column"] = Location.SourceLocation.Column,
                    ["Line"] = Location.SourceLocation.Line,
                    ["Position"] = Location.SourceLocation.Position
                }, null));

            return ret;
        }
    }
}
