using Microsoft.OpenApi.Models;
using System;

namespace Bb.Services.Chain
{


    public class HttpListenerMethodJslt : HttpListenerMethodBase
    {

        public HttpListenerMethodJslt(OperationType type, string path, string templateFileName) : base(type, path)
        {
            _templateFileName = templateFileName;
        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {

            await base.InvokeAsync(context);

            if (context.Context.Response.StatusCode != 0 && context.Context.Response.StatusCode != 200)
                return;

            bool withDebug = false;

            var dic = new Dictionary<string, Oldtonsoft.Json.Linq.JToken>();
            context.Arguments().ToList().ForEach(c => dic.Add(c.Key, Oldtonsoft.Json.Linq.JToken.FromObject(c.Value)));

            if (context.Body != null)
                dic.Add("body", Oldtonsoft.Json.Linq.JToken.FromObject(context.Body));

            var datas = context.GetDatas(this._templateFileName, withDebug, dic);

            context.Context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Context.Response.WriteAsync(datas);

        }

        public override bool Equals(object? obj)
        {
            return obj is HttpListenerMethodJslt jslt &&
                   _templateFileName == jslt._templateFileName;
        }

        private string _templateFileName;

    }

}
