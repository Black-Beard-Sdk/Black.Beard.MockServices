using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{
    public class HttpListenerPostMethod : HttpListenerMethod
    {

        public HttpListenerPostMethod(string path, string templateFileName)
            : base(OperationType.Post, path, templateFileName)
        {

        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {
            Stop();
            throw new NotImplementedException();
        }

    }


}
