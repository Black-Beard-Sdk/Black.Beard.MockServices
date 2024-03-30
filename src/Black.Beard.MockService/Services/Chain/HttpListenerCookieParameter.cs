namespace Bb.Services.Chain
{
    public class HttpListenerCookieParameter : HttpListenerParameter
    {

        public HttpListenerCookieParameter(string name, bool required) 
            : base(name, required)
        {
        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {

            //Stop();

            if (context.Context.Request.Cookies.TryGetValue(this.Name, out string value))
                context.AddArgument(this.Name, value.Trim());

            else if (this.Required)
            {
                context.Context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Context.Response.WriteAsync($"Parameter {this.Name} is required");
                return;
            }

            if (Next != null)
                await Next.InvokeAsync(context);

        }

    }


}
