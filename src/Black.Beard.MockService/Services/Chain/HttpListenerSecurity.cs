
using System.Collections.Specialized;
using System.Text;
using System.Xml.Linq;

namespace Bb.Services.Chain
{



    public class HttpListenerSecurityApprobation : HttpListenerBase
    {

        
        public HttpListenerSecurityApprobation()
        {
            this._items = new List<HttpListenerSecurity>();
        }

        public override Task InvokeAsync(HttpListenerContext context)
        {

            if (_items.Count > 0)
            {
                foreach (var item in _items)
                {
                    item.InvokeAsync(context);
                    if (context.Context.User != null)
                        return Task.CompletedTask;
                }

                context.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Context.Response.Body.Write(Encoding.UTF8.GetBytes("Unauthorized"));
            }

            return Task.CompletedTask;

        }

        public override void SetNext(HttpListenerBase next)
        {
            this._items.Add((HttpListenerSecurity)next);
        }

        private List<HttpListenerSecurity> _items;

    }



    public abstract class HttpListenerSecurity : HttpListenerChain, ICloneable
    {

        public HttpListenerSecurity(ApiKeyInEnum kind, string name) : base()
        {
            this.Kind = kind;
            this.Name = name;
            this._scopes = new List<string>();
        }

        public ApiKeyInEnum Kind { get; }

        public string Name { get; }

        public abstract object Clone();

        public List<string> Scopes => _scopes;


        private readonly List<string> _scopes;

    }


    public enum ApiKeyInEnum
    {
        Default,
        Path,
        Cookie,
        Header,
        Query
    }


}
