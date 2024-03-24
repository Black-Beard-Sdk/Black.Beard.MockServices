

using Bb.Services.Managers;

namespace Bb.Services.Chain
{


    public class HttpListenerMiddleware
    {

        public HttpListenerMiddleware(RequestDelegate next, ProjectBuilderProvider builder)
        {
            _next = next;
            _builder = builder;
            _suffix = "/" + LabelProxy;
            this.processor = new ServiceProcessor();
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var path = context.Request.Path;

            if (path.StartsWithSegments(_suffix))
            {
                var contract = GetContract(path);
                if (contract != null)
                {
                    var listener = contract.Get();

                    var ctx = new HttpListenerContext(context, this.processor);

                    await listener.InvokeAsync(ctx);
                    return;
                }
            }

            await _next(context);

        }

        private ProjectBuilderContract GetContract(PathString path)
        {

            if (path.Value != null)
            {
                var p = path.Value.Trim('/').Split('/').Skip(1).ToList();
                if (p.Count > 1)
                {

                    var contractName = p.First();
                    return _builder.Contract(contractName);

                }
            }

            return null;

        }

        private readonly RequestDelegate _next;
        private readonly ProjectBuilderProvider _builder;
        private readonly string _suffix;
        private readonly ServiceProcessor processor;
        public const string LabelProxy = "proxy";


    }

}
