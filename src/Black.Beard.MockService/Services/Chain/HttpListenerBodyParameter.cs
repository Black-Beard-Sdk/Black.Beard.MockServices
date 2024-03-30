

using Bb.ComponentModel.Accessors;
using Json.Schema;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bb.Services.Chain
{
    public class HttpListenerBodyParameter : HttpListenerParameter
    {


        public HttpListenerBodyParameter(bool required, Type? type = null, JsonSchema? schema = null)
            : base("body", required)
        {
            
            this._type = type;
            this._schema = schema;
            
            

        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {

            string bodyStr = null;
            var body = context.Context.Request.BodyReader;

            if (context.Context.Request.Body.CanRead)
            {
                
                if (_type != null)
                {

                    context.Body = await context.Context.Request.ReadFromJsonAsync(_type);

                }
                else
                {

                    System.IO.Pipelines.ReadResult r = await body.ReadAsync();
                    bodyStr = System.Text.Encoding.UTF8.GetString(r.Buffer);
                    context.Body = bodyStr;

                    if (_schema != null)
                    {

                        var options = new EvaluationOptions()
                        {
                            Culture = CultureInfo.InvariantCulture,
                            OutputFormat = OutputFormat.Hierarchical,
                        };

                        var result = _schema.Evaluate(JsonObject.Parse(bodyStr), options);
                        if (!result.IsValid)
                        {
                            context.Context.Response.StatusCode = 400;
                            context.Context.Response.ContentType = "application/json";
                            await context.Context.Response.WriteAsJsonAsync(result);
                            return;
                        }

                    }

                }

            }

            if (Next != null)
                await Next.InvokeAsync(context);

        }

        private readonly Type? _type;
        private readonly JsonSchema _schema;
    }


}
