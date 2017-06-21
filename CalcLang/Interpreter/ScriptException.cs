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
                var exType = (string)dt.GetString("ExceptionType");
                msg = (string)dt.GetString("Message");
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
                    ret.SetString(e.Key, e.Value);
                }
            }
            else
            {
                ret.SetString("Message", Payload);
            }

            var dataStackTrace = new DataTable();
            int index = 0;
            foreach (var e in ScriptStackTrace.CallStack)
            {
                var stackEntry = new DataTable();
                stackEntry.SetString("Name", e.Name);
                stackEntry.SetString("ParamCount", e.ParamCount);
                if (e.ParamNames != null)
                    stackEntry.SetString("ParamNames", new DataTable(e.ParamNames));
                else
                    stackEntry.SetString("ParamNames", new DataTable());
                stackEntry.SetString("CallLocation", new DataTable(
                    new Dictionary<string, object>()
                    {
                        ["Filename"] = e.CallLocation.FileName,
                        ["Column"] = e.CallLocation.SourceLocation.Column,
                        ["Line"] = e.CallLocation.SourceLocation.Line,
                        ["Position"] = e.CallLocation.SourceLocation.Position
                    }));
                stackEntry.SetString("HasParamsArg", e.HasParamsArg);

                dataStackTrace.SetInt(index++, stackEntry);
            }

            ret.SetString("Stacktrace", dataStackTrace);

            ret.SetString("Location", new DataTable(
                new Dictionary<string, object>()
                {
                    ["Filename"] = Location.FileName,
                    ["Column"] = Location.SourceLocation.Column,
                    ["Line"] = Location.SourceLocation.Line,
                    ["Position"] = Location.SourceLocation.Position
                }));

            return ret;
        }
    }
}
