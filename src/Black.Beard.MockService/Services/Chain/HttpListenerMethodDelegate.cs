using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{
    public class HttpListenerMethodDelegate : HttpListenerMethodBase
    {

        public HttpListenerMethodDelegate(OperationType type, string path, Func<HttpListenerContext, Task> @delegate = null)
            : base(type, path)
        {
            this._delegate = @delegate;
        }

        public HttpListenerMethodDelegate SetDelegate(Func<HttpListenerContext, Task> @delegate)
        {
            this._delegate = @delegate;
            return this;
        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {

            await base.InvokeAsync(context);

            if (!context.Context.Response.HasStarted)
                _delegate?.Invoke(context);

        }

        private Func<HttpListenerContext, Task> _delegate;

    }

}
