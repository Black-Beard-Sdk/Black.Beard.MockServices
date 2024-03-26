using Bb.ComponentModel.Accessors;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Validations;

namespace Bb.Services.Chain
{
    public class HttpListenerBodyParameter : HttpListenerParameter
    {

        public HttpListenerBodyParameter(bool required, IDictionary<string, OpenApiMediaType> contents) 
            : base("body", required)
        {
            this.Contents = contents;
            _schema = contents.Values.First().Schema;
        }

        public override async Task InvokeAsync(HttpListenerContext context)
        {

            //var txt = await context.Context.Request.Read();
            // var txt = await context.Context.Request.BodyReader.ReadAsync();
                       

            var ruleSet = new ValidationRuleSet();
            _schema.Validate(ruleSet);
            return;
        }

        private readonly IDictionary<string, OpenApiMediaType> Contents;
        private readonly OpenApiSchema _schema;
    }


}
