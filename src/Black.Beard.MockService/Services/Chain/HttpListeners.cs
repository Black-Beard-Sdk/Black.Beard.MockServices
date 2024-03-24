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

            var ctx = context.Context;

            List<HttpListenerBase> items = GetMethods(ctx);

            if (items.Count == 0)
            {
                ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                await ctx.Response.WriteAsync("Route not found");
                return;
            }

            if (items.Count > 1)
            {
                Stop();
                ctx.Response.StatusCode = StatusCodes.Status409Conflict;
                await ctx.Response.WriteAsync("Duplicate route");
                return;
            }

            await items[0].InvokeAsync(context);

        }

        private List<HttpListenerBase> GetMethods(HttpContext context)
        {

            List<HttpListenerBase> items = new List<HttpListenerBase>();

            var fullPath = context.Request.Path.Value
                .Trim('/')
                .Split('/')
                .ToArray()
                .Skip(2)
                .ToArray();


            foreach (var item in _items)
                foreach (var item2 in item.Match(fullPath, context.Request.Method))
                    items.Add(item2);

            return items;

        }

        public override void SetNext(HttpListenerBase next)
        {
            this._items.Add((HttpListener)next);
        }

        private readonly List<HttpListener> _items;

    }


}
