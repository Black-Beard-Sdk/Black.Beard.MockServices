using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{
    public class HttpListenerBodyParameter : HttpListenerParameter
    {

        public HttpListenerBodyParameter(bool required, IDictionary<string, OpenApiMediaType> contents) 
            : base("body", required)
        {
            this.Contents = contents;

        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {
            Stop();
            throw new NotImplementedException();
        }

        private readonly IDictionary<string, OpenApiMediaType> Contents;

    }


}
