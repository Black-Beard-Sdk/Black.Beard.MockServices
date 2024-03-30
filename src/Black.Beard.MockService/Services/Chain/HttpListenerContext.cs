

using Bb.Expressions;
using Bb.Services.Managers;
using System.Text;

namespace Bb.Services.Chain
{

    public class HttpListenerContext
    {

        public HttpListenerContext(HttpContext context, ServiceProcessor processor)
        {
            this.Context = context;
            this._processor = processor;
            this._arguments = new Dictionary<string, object>();
        }

        public HttpContext Context { get; }

        public object Body { get; internal set; }

        public bool TryGetArgument(string name, out object value)
        {
            return this._arguments.TryGetValue(name, out value);
        }   

        public IEnumerable<KeyValuePair<string, object>> Arguments()
        {
            return this._arguments;
        }

        public object GetArgument(string name)
        {
            if (this._arguments.ContainsKey(name))
                return this._arguments[name];
            return null;
        }

        internal void AddArgument(string name, string argument)
        {
            if (this._arguments.ContainsKey(name))
                this._arguments[name] = argument;
            else
                this._arguments.Add(name, argument);
        }

        internal string GetDatas(string templateFileName, bool withDebug, IDictionary<string, Oldtonsoft.Json.Linq.JToken> variables)
        {

            var diag = new Analysis.DiagTraces.ScriptDiagnostics();

            var reuslt = _processor.GetDatas(templateFileName, withDebug, variables, diag);

            if (!diag.Success)
            {

            }

            return reuslt;

        }

        internal void Diagnostic(string log)
        {
            this._logs.Add(log);
        }

        public void WriteLogs(StringBuilder sb)
        {
            foreach (var item in _logs)
                sb.AppendLine(item);
        }


        private List<string> _logs = new List<string>();
        private readonly Dictionary<string, object> _arguments;
        private readonly ServiceProcessor _processor;

    }

}
