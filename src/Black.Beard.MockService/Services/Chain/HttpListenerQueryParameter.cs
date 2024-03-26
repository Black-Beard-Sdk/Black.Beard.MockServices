using static Refs.System.Net;
using System.Collections.Specialized;
using System.Reflection.Metadata;

namespace Bb.Services.Chain
{
    public class HttpListenerQueryParameter : HttpListenerParameter
    {

        public HttpListenerQueryParameter(string name, bool required)
            : base(name, required)
        {

        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {

            //Stop();

            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(context.Context.Request.QueryString.ToString());

            if (queryString.AllKeys.Where(c => c == this.Name).Any())
                context.AddArgument(this.Name, queryString[this.Name]);

            else if (this.Required)
            {
                context.Context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Context.Response.WriteAsync($"Parameter {this.Name} is required");
                return;
            }

            await Next.InvokeAsync(context);

        }

    }


}
