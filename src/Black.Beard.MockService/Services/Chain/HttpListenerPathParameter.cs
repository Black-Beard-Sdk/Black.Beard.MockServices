using System.Xml.Linq;

namespace Bb.Services.Chain
{
    public class HttpListenerPathParameter : HttpListenerParameter
    {

        
        public HttpListenerPathParameter(string name, int position) 
            : base(name, true)
        {
            this.Position = position;
        }


        public static int ResolvePosition(string path, string name)
        {
            var p = path.Trim('/').Split('/');
            var position = Array.IndexOf(p, "{" + name + "}");
            return position;
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
                context.AddArgument(this.Name, argument.Trim());
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
