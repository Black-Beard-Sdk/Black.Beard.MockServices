namespace Bb.Services.Chain
{
    public class HttpListenerPathParameter : HttpListenerParameter
    {

        public HttpListenerPathParameter(string name, bool required, int position) 
            : base(name, required)
        {
            this.Position = position;
        }

        public int Position { get; }

        public override async Task InvokeAsync(HttpListenerContext context)
        {

            var array = context.Context.Request.Path.Value
                .Trim('/')
                .Split('/')
                .ToArray()
                .Skip(2)
                .ToArray();

            if (Position < array.Length)
            {
                var argument = array[Position];
                context.AddArgument(this.Name, argument);
            }
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
