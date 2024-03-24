namespace Bb.Services.Chain
{
    public class HttpListenerHeaderParameter : HttpListenerParameter
    {

        public HttpListenerHeaderParameter(string name, bool required) 
            : base(name, required)
        {
        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {
            
            Stop();

            if (context.Context.Request.Headers.TryGetValue(this.Name, out var value))
                context.AddArgument(this.Name, value);

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
