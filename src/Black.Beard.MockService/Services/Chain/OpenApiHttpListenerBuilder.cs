using Bb.Extensions;
using Bb.OpenApi;
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

            return null;

        }

        private static JsonObject CopySchema(OpenApiSchema source, string name, string suffix)
        {

            var target = new JsonObject()
            {
                ["$id"] = "http://local/" + name + suffix,
                ["$schema"] = "http://json-schema.org/draft-04/schema#",
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


    public static class JsonOpenApi
    {


        public static JsonObject GetObjects(this OpenApiDocument document, string rootKey, out JsonElement? rootModel)
        {
            var payload = document.SerializeToString().Substring(1).Trim();
            JsonElement.ObjectEnumerator modelList = JsonDocument.Parse(payload)
                .RootElement
                .GetProperty("components")
                .GetProperty("schemas")
                .EnumerateObject();

            JsonObject jsonObject = new JsonObject();
            rootModel = GetModels(rootKey, modelList, jsonObject, document);

            return jsonObject;
        }


        private static JsonElement? GetModels(string rootKey, JsonElement.ObjectEnumerator modelList, JsonObject definitions, OpenApiDocument document)
        {
            JsonElement? result = null;
            HashSet<string> hashSet = SchemaIdentifyModels.Get(document, rootKey);
            while (modelList.MoveNext())
            {
                JsonProperty current = modelList.Current;
                string name = current.Name;
                if (rootKey == name)
                {
                    result = current.Value;
                    definitions.Add(name, result.Value.AsNode());
                }
                else if (hashSet.Contains(name))
                {
                    JsonNode value = JsonNode.Parse(ConvertPath(current.Value.GetRawText()));
                    definitions.Add(name, value);
                }
            }

            if (!result.HasValue)
            {
                throw new Exception("rootKey '" + rootKey + "' not found");
            }

            return result;
        }


        private static string ConvertPath(string path)
        {

            if (path.Contains("#/components/schemas"))            
                path = path.Replace("#/components/schemas", "#/definitions");            

            return path;
        }


        public static void CopyTo(this OpenApiSchema source, JsonObject target)
        {

            target["type"] = source.Type;

            if (!string.IsNullOrEmpty(source.Format))
                target["format"] = source.Format;

            if (source.Minimum.HasValue)
                target["minimum"] = source.Minimum.Value;

            if (source.ExclusiveMinimum.HasValue)
                target["exclusiveMinimum"] = source.ExclusiveMinimum.Value;

            if (source.Maximum.HasValue)
                target["maximum"] = source.Maximum.Value;

            if (source.ExclusiveMaximum.HasValue)
                target["exclusiveMaximum"] = source.ExclusiveMaximum.Value;

            if (source.MinItems.HasValue)
                target["minItems"] = source.MinItems.Value;

            if (source.MaxItems.HasValue)
                target["mawItems"] = source.MaxItems.Value;

            if (source.AdditionalPropertiesAllowed)
                target["additionalProperties"] = true;

            if (source.ReadOnly)
                target["readOnly"] = true;

            if (source.WriteOnly)
                target["writeOnly"] = true;

            if (source.UniqueItems.HasValue)
                target["uniqueItems"] = source.UniqueItems.Value;

            if (source.MinLength.HasValue)
                target["minLength"] = source.MinLength.Value;

            if (source.MaxLength.HasValue)
                target["maxLength"] = source.MaxLength.Value;

            if (source.MultipleOf.HasValue)
                target["multipleOf"] = source.MultipleOf.Value;

            if (source.UniqueItems.HasValue)
                target["uniqueItems"] = source.UniqueItems.Value;

            if (source.Not != null)
            {

            }

            foreach (var item in source.AllOf)
            {

            }

            foreach (var item in source.OneOf)
            {

            }

            foreach (var item in source.AnyOf)
            {

            }

            foreach (var item in source.Properties)
            {

            }

            if (source.MinProperties.HasValue)
                target["minProperties"] = source.MinProperties.Value;

            if (source.MaxProperties.HasValue)
                target["maxProperties"] = source.MaxProperties.Value;


            if (!string.IsNullOrEmpty(source.Pattern))
                target["pattern"] = source.Pattern;

            if (!string.IsNullOrEmpty(source.Description))
                target["description"] = source.Description;

        }


    }

    internal class SchemaIdentifyModels : OpenApiVisitorBase, IOpenApiVisitor, IOpenApi
    {
        private readonly string[] _schemaRoots;

        private OpenApiComponents _root;

        public HashSet<string> Result { get; }

        public SchemaIdentifyModels(string[] schemaRoots)
        {
            _schemaRoots = schemaRoots;
            Result = new HashSet<string>();
        }

        public static HashSet<string> Get(OpenApiDocument schema, params string[] schemaRoots)
        {
            SchemaIdentifyModels schemaIdentifyModels = new SchemaIdentifyModels(schemaRoots);
            schema.Accept(schemaIdentifyModels);
            return schemaIdentifyModels.Result;
        }

        public override void VisitDocument(OpenApiDocument self)
        {
            _root = self.Components;
            string[] schemaRoots = _schemaRoots;
            foreach (string schemaRoot in schemaRoots)
            {
                KeyValuePair<string, OpenApiSchema> keyValuePair = self.Components.Schemas.FirstOrDefault((KeyValuePair<string, OpenApiSchema> c) => c.Key == schemaRoot);
                if (keyValuePair.Key != null)
                {
                    keyValuePair.Value.Accept(this);
                }
            }
        }

        public override void VisitSchemas(IDictionary<string, OpenApiSchema> self)
        {
            foreach (KeyValuePair<string, OpenApiSchema> item in self)
            {
                item.Value.Accept(this);
            }
        }

        public override void VisitSchema(OpenApiSchema self)
        {
            if (self.Reference != null)
            {
                string id = self.Reference.Id;
                if (!string.IsNullOrEmpty(id) && !_schemaRoots.Contains(id))
                {
                    Result.Add(id);
                }
            }

            self.Items.Accept(this);
            self.OneOf.Accept(this);
            self.AllOf.Accept(this);
            self.AnyOf.Accept(this);
            self.Properties.Accept(this);
        }

        public override void VisitSchemas(IList<OpenApiSchema> self)
        {
            foreach (OpenApiSchema item in self)
            {
                item.Accept(this);
            }
        }
    }
}
