using Bb.Extensions;
using Bb.OpenApiServices;
using Json.More;
using Json.Schema;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using SharpYaml.Tokens;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Schema;

namespace Bb.Services.Chain
{

    //https://spec.openapis.org/oas/v3.1.0#fixed-fields-22
    public class OpenApiHttpListenerBuilder : OpenApiHttpListenerBase
    {

        public OpenApiHttpListenerBuilder()
            : base()
        {

            _securities = new Dictionary<string, HttpListenerSecurity>();
            _securityAccepteds = new List<HttpListenerSecurity>();

        }


        public override HttpListenerBase? VisitDocument(OpenApiDocument self)
        {

            this._document = self;

            var result = self.Components.Accept(this);

            foreach (var item in self.SecurityRequirements)
                foreach (var security in item)
                    if (_securities.TryGetValue(security.Key.Reference.Id, out var s))
                    {
                        var clone = (HttpListenerSecurity)s.Clone();
                        foreach (var securityValue in security.Value)
                            clone.Scopes.Add(securityValue);
                        _securityAccepteds.Add(clone);
                    }

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

            HttpListenerMethodBase method = null;

            var templateFilename = Context.GetDataFor(self).GetData<string>("templateName");

            var path = this.Contexts[1];
            var methodType = (OperationType)Enum.Parse(typeof(OperationType), this.Contexts[0]);
            switch (methodType)
            {

                case OperationType.Get:
                case OperationType.Put:
                case OperationType.Post:
                case OperationType.Delete:
                case OperationType.Options:
                case OperationType.Head:
                case OperationType.Patch:
                case OperationType.Trace:

                    method = new HttpListenerMethodJslt(methodType, path, templateFilename);

                    HttpListenerParameter result = self.Parameters.Accept(this) as HttpListenerParameter;
                    if (result != null)
                        method.AddParameter(result);

                    result = self.RequestBody.Accept(this) as HttpListenerParameter;
                    if (result != null)
                        method.AddParameter(result);

                    break;

                default:
                    Stop();
                    break;
            }

            if (method != null)
            {

                foreach (var item in this._securityAccepteds)
                {
                    var s = (HttpListenerSecurity)item.Clone();
                    method.SetSecurity(s);
                }

                var security = self.Security.Accept(this);
                if (security != null)
                    method.SetSecurity(security);

            }

            return method;

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
                    var position = HttpListenerPathParameter.ResolvePosition(this.Contexts[1], name);
                    return new HttpListenerPathParameter(name, position);

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

            using (var i = Store())
            {
                self.Content.Accept(this);
                i.TryGetInStorage("content", out var content);
                var schema = JsonSchema.FromText(content.ToString());
                return new HttpListenerBodyParameter(self.Required, null, schema);
            }

        }

        public override HttpListenerBase? VisitComponents(OpenApiComponents self)
        {
            self.SecuritySchemes.Accept(this);
            return null;
        }

        #region security

        public override HttpListenerBase? VisitSecuritySchemes(IDictionary<string, OpenApiSecurityScheme> self)
        {


            foreach (var item in self)
            {

                var security = (HttpListenerSecurity)item.Value.Accept(this);

                if (_securities.ContainsKey(item.Key))
                    _securities[item.Key] = security;
                else
                    _securities.Add(item.Key, security);

            }

            return null;

        }

        public override HttpListenerBase? VisitSecurityScheme(OpenApiSecurityScheme self)
        {

            /*
             components:
              securitySchemes:
                BasicAuth:
                  type: http
                  scheme: basic
                BearerAuth:
                  type: http
                  scheme: bearer
                ApiKeyAuth:
                  type: apiKey
                  in: header
                  name: X-API-Key
                OpenID:
                  type: openIdConnect
                  openIdConnectUrl: https://example.com/.well-known/openid-configuration
                OAuth2:
                  type: oauth2
                  flows:
                    authorizationCode:
                      authorizationUrl: https://example.com/oauth/authorize
                      tokenUrl: https://example.com/oauth/token
                      scopes:
                        read: Grants read access
                        write: Grants write access
                        admin: Grants access to admin operations
             */

            // self.Flows.Accept(this);

            // https://spec.openapis.org/oas/v3.1.0#fixed-fields-22          
            ApiKeyInEnum kind = Enum.Parse<ApiKeyInEnum>(self.In.ToString());

            switch (self.Type)
            {
                case SecuritySchemeType.ApiKey:
                    return new HttpListenerSecurityApiKeyAuth(kind, self.Name);

                case SecuritySchemeType.Http:
                    Stop();
                    return new HttpListenerSecurityHttp(kind, self.Name);

                case SecuritySchemeType.OAuth2:
                    Stop();
                    return new HttpListenerSecurityOAuth2(kind, self.Name);

                case SecuritySchemeType.OpenIdConnect:
                    Stop();
                    return new HttpListenerSecurityOpenIdConnect(kind, self.Name);

                default:
                    Stop();
                    break;
            }

            Stop();
            throw new NotImplementedException();
        }

        public override HttpListenerBase? VisitSecurityRequirements(IList<OpenApiSecurityRequirement> self)
        {

            if (self.Count > 1)
                Stop();

            return null;
        }

        public override HttpListenerBase? VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            Stop();
            throw new NotImplementedException();
        }

        #endregion security

        public override HttpListenerBase? VisitMediaTypes(IDictionary<string, OpenApiMediaType> self)
        {

            foreach (var item in self)
            {
                item.Value.Accept(this);
            }

            return null;

        }

        public override HttpListenerBase? VisitMediaType(OpenApiMediaType self)
        {
            self.Schema.Accept(this);
            return null;
        }

        public override HttpListenerBase? VisitSchema(OpenApiSchema self)
        {
            JsonObject schema = null;
            JsonObject definitions = null;
            JsonElement? root = null;
            string name = null;
            string suffix = string.Empty;
            string type = self.Type;
            if (self.Type == "array")
            {
                suffix = "List";
                name = self.Items.Reference.Id;
                definitions = _document.GetObjects(name, out root);
                schema = CopySchema(self, name, suffix);

                schema["items"] = new JsonObject()
                {
                    ["$ref"] = "#/definitions/" + name
                };

            }
            else if (self.Type == "object")
            {
                Stop();
                name = self.Reference.Id;
                definitions = OpenApiExtension.GetSchemaObjects(_document, name, out root);
                schema = CopySchema(self, name, suffix);
            }

            schema["definitions"] = definitions;
            Store("content", schema);


            // var txt = schema.ToString();

            return null;

        }

        private static JsonObject CopySchema(OpenApiSchema source, string name, string suffix)
        {

            var target = new JsonObject()
            {
                ["$id"] = "http://local/" + name + suffix,
                ["$schema"] = "https://json-schema.org/draft/2019-09/schema",
                ["title"] = name + suffix,                
            };

            source.CopyTo(target);

            return target;

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

        public override HttpListenerBase? VisitSecurityScheme(KeyValuePair<OpenApiSecurityScheme, IList<string>> self)
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
        private Dictionary<string, HttpListenerSecurity> _securities;
        private List<HttpListenerSecurity> _securityAccepteds;
    }
}
