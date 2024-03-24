using Bb.Extensions;
using Bb.OpenApiServices;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{

    public class OpenApiHttpListener : OpenApiHttpListenerBase
    {

        public OpenApiHttpListener()
            : base()
        {

        }


        public override HttpListenerBase? VisitDocument(OpenApiDocument self)
        {
            return self.Paths.Accept(this);
        }


        public override HttpListenerBase? VisitPaths(OpenApiPaths self)
        {

            HttpListeners result = new HttpListeners();

            foreach (var item in self)
                using (var scope = PushContext(item.Key))
                    result.SetNext((HttpListener)item.Accept(this));

            return result;

        }

        public override HttpListenerBase? VisitPathItem(OpenApiPathItem self)
        {
            var r = self.Operations.Accept(this);
            return r;
        }

        public override HttpListenerBase? VisitOperations(IDictionary<OperationType, OpenApiOperation> self)
        {

            var path = this.Contexts[0];

            HttpListener result = new HttpListener(path.Trim('/'));

            foreach (var item in self)
                using (var scope = PushContext(item.Key.ToString()))
                    result.SetNext(item.Accept(this));

            return result;

        }

        public override HttpListenerBase? VisitOperation(OpenApiOperation self)
        {

            HttpListenerBase result = self.Parameters.Accept(this);

            if (result == null)
                result = self.RequestBody.Accept(this);

            if (result == null)
            {
                Stop();
                throw new NotImplementedException();
            }

            var templateFilename = Context.GetDataFor(self).GetData<string>("templateName");
            //if (!string.IsNullOrEmpty(templateFilename))
            //    templateFilename = Context.GetRelativePath(templateFilename);

            var path = this.Contexts[1];
            var method = (OperationType)Enum.Parse(typeof(OperationType), this.Contexts[0]);
            switch (method)
            {
                case OperationType.Get:
                    result.SetNext(new HttpListenerGetMethod(path, templateFilename));
                    break;

                case OperationType.Put:
                    Stop();
                    break;

                case OperationType.Post:
                    result.SetNext(new HttpListenerPostMethod(path, templateFilename));
                    break;

                case OperationType.Delete:
                    Stop();
                    break;

                case OperationType.Options:
                    Stop();
                    break;

                case OperationType.Head:
                    Stop();
                    break;

                case OperationType.Patch:
                    Stop();
                    break;

                case OperationType.Trace:
                    Stop();
                    break;

                default:
                    Stop();
                    break;
            }

            return result;

        }

        public override HttpListenerBase? VisitParameters(IList<OpenApiParameter> self)
        {

            HttpListenerBase parameter = null;
            foreach (var item in self)
            {
                var p = item.Accept(this);

                if (parameter == null)
                    parameter = p;
                else
                    parameter.SetNext(p);
            }

            return parameter;

        }

        public override HttpListenerBase? VisitParameters(IDictionary<string, OpenApiParameter> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitParameter(OpenApiParameter self)
        {

            string name = self.Name;
            bool required = self.Required;

            switch (self.In)
            {
                case ParameterLocation.Query:
                    return new HttpListenerQueryParameter(name, required);

                case ParameterLocation.Header:
                    return new HttpListenerHeaderParameter(name, required);
                    ;
                case ParameterLocation.Path:
                    var path = this.Contexts[1];
                    var p = path.Trim('/').Split('/');
                    var position = Array.IndexOf(p, "{" + name + "}");

                    return new HttpListenerPathParameter(name, required, position);

                case ParameterLocation.Cookie:
                    return new HttpListenerCookieParameter(name, required);

                default:
                    break;
            }

            Stop();
            throw new NotImplementedException();

        }

        public override HttpListenerBase? VisitRequestBody(OpenApiRequestBody self)
        {                       
            return new HttpListenerBodyParameter(self.Required, self.Content);       
        }

        public override HttpListenerBase? VisitPathItems(Dictionary<RuntimeExpression, OpenApiPathItem> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitCallback(OpenApiCallback self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitCallbacks(IDictionary<string, OpenApiCallback> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitComponents(OpenApiComponents self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitContact(OpenApiContact self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitEncoding(OpenApiEncoding self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitEncodings(IDictionary<string, OpenApiEncoding> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitEnumPrimitive(IOpenApiPrimitive self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitExample(OpenApiExample self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitExamples(IDictionary<string, OpenApiExample> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitExtension(IOpenApiExtension self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitExtensions(IDictionary<string, IOpenApiExtension> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitExternalDocs(OpenApiExternalDocs self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitHeader(OpenApiHeader self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitHeaders(IDictionary<string, OpenApiHeader> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitInfo(OpenApiInfo self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitLicense(OpenApiLicense self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitLink(OpenApiLink self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitLinks(IDictionary<string, OpenApiLink> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitMediaType(OpenApiMediaType self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitMediaTypes(IDictionary<string, OpenApiMediaType> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitReference(OpenApiReference self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitRequestBodies(IDictionary<string, OpenApiRequestBody> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitResponse(OpenApiResponse self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitResponses(IDictionary<string, OpenApiResponse> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitRuntimeExpression(RuntimeExpression self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitRuntimeExpressionWrapper(RuntimeExpressionAnyWrapper self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitRuntimeExpressionWrappers(Dictionary<string, RuntimeExpressionAnyWrapper> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSchema(OpenApiSchema self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSchemas(IList<OpenApiSchema> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSchemas(IDictionary<string, OpenApiSchema> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSecurityRequirements(IList<OpenApiSecurityRequirement> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSecurityScheme(KeyValuePair<OpenApiSecurityScheme, IList<string>> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSecuritySchemes(IDictionary<string, OpenApiSecurityScheme> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitServer(OpenApiServer self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitServers(IList<OpenApiServer> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitTag(OpenApiTag self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitTags(IList<OpenApiTag> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitVariable(OpenApiServerVariable self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitVariables(IDictionary<string, OpenApiServerVariable> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        private readonly ContextGenerator _ctx;


    }
}
