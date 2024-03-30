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

            if (context.Context.Request.Headers.TryGetValue(this.Name, out var value))
            {

                if (value.Count == 1)
                    context.AddArgument(this.Name, value[0].Trim());

                else
                {
                    Stop();
                }

                

            }

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
