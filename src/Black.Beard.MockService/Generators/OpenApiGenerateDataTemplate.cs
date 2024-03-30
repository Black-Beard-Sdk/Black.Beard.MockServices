
using Bb.Expressions;
using Bb.Json.Jslt.Asts;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Bb.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Expressions;
using Bb.Http;

namespace Bb.OpenApiServices
{

    public class OpenApiGenerateDataTemplate : OpenApiGeneratorJsltBase
    {


        public OpenApiGenerateDataTemplate()
        {

        }


        public override JsltBase VisitDocument(OpenApiDocument self)
        {
            _self = self;
            return self.Paths.Accept(this);
        }

        public override JsltBase? VisitPaths(OpenApiPaths self)
        {

            foreach (var item in self)
                item.Accept(this);

            return default;

        }


        public override JsltBase VisitOperation(OpenApiOperation self)
        {

            foreach (var item in ResolveResponseSchemas(self, "2"))
            {
                GenerateTemplate(self, item, string.Empty);
            }

            //foreach (var item in ResolveResponseSchemas(self, "4"))
            //{
            //    GenerateTemplate(self, item, "400");
            //}

            //foreach (var item in ResolveResponseSchemas(self, "5"))
            //{
            //    GenerateTemplate(self, item, "500");
            //}

            return null;

        }

        public override JsltBase? VisitSchemas(IList<OpenApiSchema> self)
        {

            Stop();

            foreach (var item in self)
                item.Accept(this);

            return default;

        }

        public override JsltBase? VisitSchemas(IDictionary<string, OpenApiSchema> self)
        {

            Stop();
            
            foreach (var item in self)
                item.Accept(this);

            return default;
        
        }

        public override JsltBase VisitSchema(OpenApiSchema self)
        {

            var typeName = self.Type;

            if (typeName == null)
            {

                if (self.Properties.Any())
                    typeName = "object";

                else if (self.Items != null)
                    typeName = "array";

            }

            if (typeName == "object")
            {

                var required2 = this._code.CurrentBlock.Datas.GetData<bool>("required");
                var result = new JsltObject();

                foreach (var item in self.Properties)
                {
                    using (var current = this._code.Stack())
                    {
                        var required = self.Required.Select(c => c == item.Key).Any();
                        current.Datas.SetData("required", required);

                        using (var p = this.PushContext("property"))
                        {

                            var value = item.Accept(this);

                            var property = new JsltProperty()
                            {
                                Name = item.Key,
                                Value = value,
                            };

                            result.Append(property);

                        }
                    }
                }

                return result;

            }

            else if (typeName == "array")
            {
                if (self.Items != null)
                {
                    var item1 = self.Items.Accept(this);
                    var result = new JsltArray(1);
                    result.Items.Add(item1);
                    return result;
                }
                Stop();
            }

            else if (typeName == "string")
            {

                //var result = new JsltObject();
                //var d = new JsltDirectives()
                //    .SetCulture(CultureInfo.CurrentCulture)
                //    .Output(c => c.SetFilter("$.value"))
                //    ;
                //result.Append(d);

                JsltBase value = null;
                switch (self?.Format?.ToLower() ?? string.Empty)
                {

                    case "float":
                        value = new JsltFunctionCall("getrandom_float"
                            , new JsltConstant(self?.Minimum, JsltKind.Float), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                        );
                        break;

                    case "int32":
                        value = new JsltFunctionCall("getrandom_integer"
                            , new JsltConstant(self?.Minimum, JsltKind.Integer), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                        );
                        break;

                    case "date":
                    case "date-time":
                        value = new JsltFunctionCall("getrandom_datatime");
                        break;

                    case "email":
                        value = new JsltFunctionCall("getrandom_email");
                        break;

                    case "hostname":
                        value = new JsltFunctionCall("getrandom_hostname");
                        break;

                    case "ipv4":
                        value = new JsltFunctionCall("getrandom_ipv4");
                        break;

                    case "ipv6":
                        value = new JsltFunctionCall("getrandom_ipv6");
                        break;

                    case "uri":
                        value = new JsltFunctionCall("getrandom_uri");
                        break;

                    case "binary":
                        value = new JsltFunctionCall("getrandom_binary", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                        break;

                    case "password":
                        value = new JsltFunctionCall("getrandom_password", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                        break;

                    case "uuid":
                        value = new JsltFunctionCall("uuid");
                        break;

                    default:
                    case "":
                        value = new JsltFunctionCall("getrandom_string"
                           , new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer)
                           , new JsltConstant(self.Pattern, JsltKind.String)
                        );
                        break;
                }

                //result.Append(new JsltProperty() { Name = "value", Value = value });
                //return result;
                return value;
            }

            else if (typeName == "boolean")
            {

                //var result = new JsltObject();
                //var d = new JsltDirectives()
                //    .SetCulture(CultureInfo.CurrentCulture)
                //    .Output(c => c.SetFilter("$.value"))
                //    ;
                //result.Append(d);
                var value = new JsltFunctionCall("getrandom_boolean");
                //result.Append(new JsltProperty() { Name = "value", Value = value });
                //return result;
                return value;
            }

            else if (typeName == "integer")
            {

                //var result = new JsltObject();
                //var d = new JsltDirectives()
                //    .SetCulture(CultureInfo.CurrentCulture)
                //    .Output(c => c.SetFilter("$.value"))
                //    ;
                //result.Append(d);
                var value = new JsltFunctionCall("getrandom_integer"
                            , new JsltConstant(self?.Minimum, JsltKind.Integer), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                        );
                //result.Append(new JsltProperty() { Name = "value", Value = value });
                //return result;
                return value;
            }

            Stop();
            throw new NotImplementedException();

        }

        public JsltBase VisitSchema2(OpenApiSchema self)
        {

            var typeName = self.Type;

            if (typeName == null)
            {

                if (self.Properties.Any())
                    typeName = "object";

                else if (self.Items != null)
                    typeName = "array";

            }

            if (typeName == "array")
            {
                if (self.Items != null)
                {
                    var item1 = self.Items.Accept(this);
                    var result = new JsltArray(1);
                    result.Items.Add(item1);
                    return result;
                }
            }

            else if (typeName == "object")
            {
                var value = self.Accept(this);
                return value;

            }

            else
            {

                JsltBase result = null;

                if (self.Enum.Count > 0)
                {
                    List<JsltBase> list = new List<JsltBase>(self.Enum.Count);
                    foreach (IOpenApiAny item in self.Enum)
                    {
                        if (item is IOpenApiPrimitive p)
                            list.Add(p.Accept(this));

                        else
                        {
                            Stop();
                        }
                    }
                    result = new JsltFunctionCall("getrandom_in_list", new JsltArray(list));
                }
                else
                {

                    var type = self.ResolveType(out var schema2);

                    switch (self?.Format?.ToLower() ?? string.Empty)
                    {

                        case "float":
                            result = new JsltFunctionCall("getrandom_float"
                                , new JsltConstant(self?.Minimum, JsltKind.Float), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                            );
                            break;

                        case "int32":
                            result = new JsltFunctionCall("getrandom_integer"
                                , new JsltConstant(self?.Minimum, JsltKind.Integer), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                            );
                            break;

                        case "date":
                        case "date-time":
                            result = new JsltFunctionCall("getrandom_datatime");
                            break;

                        case "email":
                            result = new JsltFunctionCall("getrandom_email");
                            break;

                        case "hostname":
                            result = new JsltFunctionCall("getrandom_hostname");
                            break;

                        case "ipv4":
                            result = new JsltFunctionCall("getrandom_ipv4");
                            break;

                        case "ipv6":
                            result = new JsltFunctionCall("getrandom_ipv6");
                            break;

                        case "uri":
                            result = new JsltFunctionCall("getrandom_uri");
                            break;

                        case "binary":
                            result = new JsltFunctionCall("getrandom_binary", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                            break;

                        case "password":
                            result = new JsltFunctionCall("getrandom_password", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                            break;

                        case "uuid":
                            result = new JsltFunctionCall("uuid");
                            break;


                        case "":
                            switch (type.ToLower())
                            {
                                case "string":
                                    JsltBase[] arguments = new JsltBase[]
                                    {
                                        new JsltConstant(self.MinLength, JsltKind.Integer)
                                      , new JsltConstant(self.MaxLength, JsltKind.Integer)
                                      , new JsltConstant(self.Pattern, JsltKind.String)
                                    };
                                    result = new JsltFunctionCall("getrandom_string", arguments);
                                    break;

                                case "int32":
                                    result = new JsltFunctionCall("getrandom_integer"
                                           , new JsltConstant(self.Minimum, JsltKind.Integer), new JsltConstant(self.Maximum, JsltKind.Integer)
                                           , new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                                    );
                                    break;

                                case "boolean":
                                    result = new JsltFunctionCall("getrandom_boolean");
                                    break;

                                default:
                                    result = new JsltFunctionCall("getrandom_" + type
                                           , new JsltConstant(self.MinLength, JsltKind.Integer)
                                           , new JsltConstant(self.MaxLength, JsltKind.Integer)
                                           , new JsltConstant(self.Pattern, JsltKind.String)
                                           , new JsltConstant(self.Minimum, JsltKind.Integer)
                                           , new JsltConstant(self.Maximum, JsltKind.Integer)
                                           , new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean)
                                           , new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                                    );
                                    break;
                            }
                            break;

                        default:
                            break;
                    }

                }
                if (result != null)
                    return result;

            }

            Stop();
            throw new NotImplementedException();

        }

        public override JsltConstant VisitEnumPrimitive(IOpenApiPrimitive self)
        {

            switch (self.PrimitiveType)
            {
                case PrimitiveType.Integer:
                    var e1 = self as OpenApiInteger;
                    return new JsltConstant(e1.Value, JsltKind.Integer);

                case PrimitiveType.String:
                    var e2 = self as OpenApiString;
                    return new JsltConstant(e2.Value, JsltKind.String);

                case PrimitiveType.Long:
                    Stop();
                    break;
                case PrimitiveType.Float:
                    Stop();
                    break;
                case PrimitiveType.Double:
                    Stop();
                    break;

                case PrimitiveType.Byte:
                    Stop();
                    break;
                case PrimitiveType.Binary:
                    Stop();
                    break;
                case PrimitiveType.Boolean:
                    Stop();
                    break;
                case PrimitiveType.Date:
                    Stop();
                    break;
                case PrimitiveType.DateTime:
                    Stop();
                    break;
                case PrimitiveType.Password:
                    Stop();
                    break;
                default:
                    Stop();
                    break;
            }

            Stop();
            throw new NotImplementedException();

        }              

        public override JsltBase VisitParameter(OpenApiParameter self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitResponse(OpenApiResponse self)
        {
            Stop();
            throw new NotImplementedException();
        }               

        public override JsltBase VisitComponents(OpenApiComponents self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitInfo(OpenApiInfo self)
        {
            Stop();
            throw new NotImplementedException();
        }               

        public override JsltBase VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitCallback(OpenApiCallback self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitCallbacks(IDictionary<string, OpenApiCallback> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitContact(OpenApiContact self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitEncoding(OpenApiEncoding self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitEncodings(IDictionary<string, OpenApiEncoding> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitExample(OpenApiExample self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitExamples(IDictionary<string, OpenApiExample> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitExtension(IOpenApiExtension self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitExtensions(IDictionary<string, IOpenApiExtension> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitExternalDocs(OpenApiExternalDocs self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitHeader(OpenApiHeader self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitHeaders(IDictionary<string, OpenApiHeader> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitLicense(OpenApiLicense self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitLink(OpenApiLink self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitLinks(IDictionary<string, OpenApiLink> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitMediaTypes(IDictionary<string, OpenApiMediaType> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitMediaType(OpenApiMediaType self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitOperation(KeyValuePair<OperationType, OpenApiOperation> self)
        {

            string key = self.Key.ToString();
            OpenApiOperation value = self.Value;

            foreach (var item in ResolveResponseSchemas(value, "2"))
                GenerateTemplate(value, item, string.Empty);

            //foreach (var item in ResolveResponseSchemas(value, "4"))
            //    GenerateTemplate(value, item, "400");

            //foreach (var item in ResolveResponseSchemas(value, "5"))
            //    GenerateTemplate(value, item, "500");

            return null;
        
        }

        public override JsltBase? VisitOperations(IDictionary<OperationType, OpenApiOperation> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitParameters(IDictionary<string, OpenApiParameter> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitParameters(IList<OpenApiParameter> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitPathItem(KeyValuePair<RuntimeExpression, OpenApiPathItem> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitPathItem(OpenApiPathItem self)
        {

            foreach (var item in self.Operations)
                item.Accept(this);

            return default;

        }

        public override JsltBase? VisitPathItems(Dictionary<RuntimeExpression, OpenApiPathItem> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitReference(OpenApiReference self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitRequestBodies(IDictionary<string, OpenApiRequestBody> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitRequestBody(OpenApiRequestBody self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitResponses(IDictionary<string, OpenApiResponse> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitRuntimeExpression(RuntimeExpression self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitRuntimeExpressionWrapper(RuntimeExpressionAnyWrapper self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitRuntimeExpressionWrappers(Dictionary<string, RuntimeExpressionAnyWrapper> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitSecurityRequirements(IList<OpenApiSecurityRequirement> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitSecuritySchemes(IDictionary<string, OpenApiSecurityScheme> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitServers(IList<OpenApiServer> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitTags(IList<OpenApiTag> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitVariable(OpenApiServerVariable self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitVariables(IDictionary<string, OpenApiServerVariable> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase? VisitSecurityScheme(KeyValuePair<OpenApiSecurityScheme, IList<string>> self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitServer(OpenApiServer self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitTag(OpenApiTag self)
        {
            Stop();
            throw new NotImplementedException();
        }

        private static string ResolveName(OpenApiSchema item)
        {
            string name = "";
            var reference = item.Reference;
            if (reference != null && reference.Id != null)
                name = reference.Id;
            else
            {
                var o = item.Items;
                if (o != null)
                {
                    reference = o.Reference;
                    if (reference != null && reference.Id != null)
                        name = reference.Id;
                }
                else
                {

                }
            }

            return name;
        }

        private class Response
        {
            public string Code { get; internal set; }
            public OpenApiSchema Schema { get; internal set; }
            public string Kind { get; internal set; }
        }
        
        private IEnumerable<Response> ResolveResponseSchemas(OpenApiOperation self, string code)
        {

            string result = null;

            List<Response> _resultTypes = new List<Response>();
            foreach (var item2 in self.Responses)
                if (item2.Key.StartsWith(code))
                {
                    var content = item2.Value.Content.FirstOrDefault();
                    var t2 = content.Value;
                    if (t2 != null)
                    {
                        OpenApiSchema t = t2.Schema;
                        var v = t.ResolveType(out var schema2);
                        if (v != null)
                        {
                            if (schema2 != null)
                            {
                                Stop();
                            }
                            _resultTypes.Add(new Response() { Code = item2.Key, Schema = t, Kind = content.Key });
                        }
                    }
                }

            var item3 = _resultTypes.OrderBy(c => c.Code);

            return item3;

        }

        private void GenerateTemplate(OpenApiOperation self, Response item, string code)
        {

            string name = ResolveName(item.Schema);
            var templateName = $"template_{name}_jslt.json";

            if (item.Kind == "application/json")
            {
                var content = item.Schema.Accept(this);

                var target = Context.AppendDocument("Templates", templateName, content.ToString());
                var bag = Context.GetDataFor(self);
                bag.SetData("templateName" + code, target);

            }
            else
            {
                Stop();
            }
        }

        //private bool error;

    }


}