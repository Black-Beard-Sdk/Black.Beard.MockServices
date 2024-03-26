using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{
    public class HttpListenerMethod : HttpListenerChain
    {

        public HttpListenerMethod(OperationType type, string path, string templateFileName) : base()
        {
            this.Type = type;
            this.Path = path.Split('/');
            TemplateFileName = templateFileName;
        }


        public override async Task InvokeAsync(HttpListenerContext context)
        {

            bool withDebug = false;

            var dic = new Dictionary<string, Oldtonsoft.Json.Linq.JToken>();
            context.Arguments().ToList().ForEach(c => dic.Add(c.Key, Oldtonsoft.Json.Linq.JToken.FromObject(c.Value)));

            var datas = context.GetDatas(this.TemplateFileName, withDebug, dic);

            context.Context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Context.Response.WriteAsync(datas);


        }


        public OperationType Type { get; }

        public string[] Path { get; }
        
        public string TemplateFileName { get; }

    }


}
