using System.Text;

namespace Bb.Services.Chain
{
    public class HttpListeners : HttpListenerBase
    {


        public HttpListeners() : base()
        {
            this._items = new List<HttpListener>();
        }


        public override async Task InvokeAsync(HttpListenerContext context)
        {
            StringBuilder sb;
            List<HttpListenerBase> items = GetMethods(context);
            var ctx = context.Context;

            if (items.Count == 0)
            {
                sb = new StringBuilder();
                sb.AppendLine("Route not found");
                context.WriteLogs(sb);
                ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                await ctx.Response.WriteAsync(sb.ToString());
                return;
            }

            if (items.Count > 1)
            {
                Stop();
                sb = new StringBuilder();
                sb.AppendLine("To many routes");
                context.WriteLogs(sb);
                ctx.Response.StatusCode = StatusCodes.Status409Conflict;
                await ctx.Response.WriteAsync(sb.ToString());
                return;
            }

            await items[0].InvokeAsync(context);

        }

        private List<HttpListenerBase> GetMethods(HttpListenerContext context)
        {

            List<HttpListenerBase> items = new List<HttpListenerBase>();

            var fullPath = context.Context.Request.Path.Value
                .Trim('/')
                .Split('/')
                .ToArray()
                .Skip(2)
                .ToArray();


            foreach (var item in _items)
                foreach (var item2 in item.Match(fullPath, context))
                    items.Add(item2);

            return items;

        }

        public override void SetNext(HttpListenerBase next)
        {
            this._items.Add((HttpListener)next);
        }

        private readonly List<HttpListener> _items;

        public ProjectBuilderContract Contract { get; internal set; }
    }


}
