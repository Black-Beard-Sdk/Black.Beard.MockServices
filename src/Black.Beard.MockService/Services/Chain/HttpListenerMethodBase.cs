using Microsoft.OpenApi.Models;
using System.Reflection.Metadata;

namespace Bb.Services.Chain
{
    public abstract class HttpListenerMethodBase : HttpListenerChain
    {

        public HttpListenerMethodBase(OperationType type, string path) : base()
        {
            this.Type = type;
            this.PathToString = path;
            this.Path = path.Split('/');
            this._security = new HttpListenerSecurityApprobation();
        }

        public override void SetNext(HttpListenerBase next)
        {

            if (next is HttpListenerParameter)
                AddParameter((HttpListenerParameter)next);

            else if (next is HttpListenerSecurity security)
                SetSecurity(security);

            else
                base.SetNext(next);

        }

        public override Task InvokeAsync(HttpListenerContext context)
        {

            if (_parameters != null)
                _parameters.InvokeAsync(context);

            if (!context.Context.Response.HasStarted)
                if (_security != null)
                    _security.InvokeAsync(context);

            return Task.CompletedTask;

        }

        public void AddParameter(HttpListenerParameter parameter)
        {
            if (_parameters == null)
                _parameters = parameter;
            else
                _parameters.SetNext(parameter);
        }

        internal void SetSecurity(HttpListenerBase security)
        {
            _security.SetNext(security);
        }

        public override bool Equals(object? obj)
        {
            return obj is HttpListenerMethodBase @base &&
                   EqualityComparer<HttpListenerBase>.Default.Equals(_security, @base._security);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_security);
        }

        public OperationType Type { get; }
        public string PathToString { get; }
        public string[] Path { get; }

        private HttpListenerParameter _parameters;
        private HttpListenerBase _security;

    }

}
