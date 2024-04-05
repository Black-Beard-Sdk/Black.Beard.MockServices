using Microsoft.OpenApi.Models;
using System;
using System.Diagnostics;

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
            string datas = null;
            await base.InvokeAsync(context);

            if (context.Context.Response.StatusCode != 0 && context.Context.Response.StatusCode != 200)
                return;

            bool withDebug = false;

            var dic = new Dictionary<string, object>();
            foreach (var item in context.Arguments())
                dic.Add(item.Key, item.Value);

            if (context.Body != null)
                dic.Add("body", context.Body);

            var diag = new Analysis.DiagTraces.ScriptDiagnostics();

            try
            {
                datas = context.GetDatas(this._templateFileName, withDebug, dic, diag);
            }
            catch (Exception ex)
            {
                LocalDebug.Stop();
                context.Context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Context.Response.WriteAsync(ex.ToString());
                return;
            }

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
