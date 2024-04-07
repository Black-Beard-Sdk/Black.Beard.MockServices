using Bb.Extensions;
using Bb.Jslt.Asts;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApiServices
{

    public class ModelAnalyze : OpenApiGenericVisitor<Object>
    {

        public ModelAnalyze(ContextGenerator ctx)
        {
            this._ctx = ctx;
        }

        public override object? VisitDocument(OpenApiDocument self)
        {
            this._document = self;
            self.Paths.Accept(this);
            return null;
        }

        public override object? VisitPaths(OpenApiPaths self)
        {

            foreach (var item in self)
                item.Value.Accept(this);

            return null;
        }

        public override object? VisitPathItem(OpenApiPathItem self)
        {

            foreach (var item in self.Operations)
                using (var store = NewStore())
                    item.Value.Accept(this);

            return null;
        }

        public override object? VisitOperation(OpenApiOperation self)
        {
            // create layers
            Dictionary<string, Layer> responses = new Dictionary<string, Layer>();
            List<Layer> parameters = (List<Layer>)self.Parameters.Accept(this);

            var o = self.RequestBody?.Accept(this);
            if (o != null)
                parameters.Add((Layer)o);

            foreach (var item in self.Responses)
            {
                o = item.Value.Accept(this);
                if (o != null)
                    responses.Add(item.Key, (Layer)o);
            }
            foreach (var response in responses)
                foreach (var parameter in parameters)
                    parameter.Evaluate(response.Value, _ctx);

            return null;
        }

        public override object? VisitRequestBody(OpenApiRequestBody self)
        {

            var layer = new Layer();

            using (var store = NewStore())
            {

                store.AddInStorage("schema", self);
                store.AddInStorage("layer", layer);

                foreach (var item in self.Content.Keys)
                    if (item == "application/json")
                        layer = (Layer)self.Content[item].Accept(this);

            }

            layer.Source = "body";

            return layer;

        }

        public override object? VisitResponse(OpenApiResponse self)
        {

            var layer = new Layer();

            using (var store = NewStore())
            {
                store.AddInStorage("schema", self);
                store.AddInStorage("layer", layer);
                foreach (var item in self.Content.Keys)
                    if (item == "application/json")
                        layer = (Layer)self.Content[item].Accept(this);
            }

            return layer;

        }

        public override object? VisitParameters(IList<OpenApiParameter> self)
        {
            List<Layer> layers = new List<Layer>();
            foreach (var item in self)
                layers.Add((Layer)item.Accept(this));

            return layers;
        }


        public override object? VisitParameter(OpenApiParameter self)
        {
            var layer = new Layer()
            {
                Name = self.Name,
                Type = "string",
                Instance = self,
                Source = self.In.ToString(),
            };

            return layer;
        }


        public override object? VisitMediaType(OpenApiMediaType self)
        {
            return self.Schema.Accept(this);
        }

        public override object? VisitSchema(OpenApiSchema self)
        {

            string? typeName = ResolveTypeName(self);
            var layer = new Layer()
            {
                Type = typeName,
                Instance = self,
            };

            base.TryGetInStorage("layer", out Layer parentLayer);

            using (var store = base.NewStore())
            {

                store.AddInStorage("schema", self);
                store.AddInStorage("layer", layer);

                if (parentLayer != null)
                    parentLayer.AddLayer(layer);

                switch (typeName)
                {

                    case "object":
                        OpenApiReference reference = null;
                        var s = self;
                        if (s.Reference != null)
                        {
                            reference = s.Reference;
                        }
                        else if (s.Items.Reference != null)
                        {
                            s = self.Items;
                            reference = s.Reference;
                        }

                        if (reference != null)
                            layer.Name = reference.Id;
                        else
                            layer.Name = "Noname";

                        foreach (var item in s.Properties)
                        {
                            var l = (Layer)item.Accept(this);
                            l.Name = item.Key;
                        }
                        break;

                    case "array":
                        self.Items.Accept(this);
                        break;

                    case "string":
                    case "boolean":
                    case "integer":
                        break;

                    default:
                        Stop();
                        break;

                }

            }

            return layer;

        }

        private string ResolveTypeName(OpenApiSchema self)
        {

            var typeName = self.Type;

            if (typeName == null)
            {
                if (self.Properties.Any())
                    typeName = "object";

                else if (self.Items != null)
                    typeName = "array";

                else
                {
                    Stop();
                }
            }

            return typeName;

        }



        public override object? VisitCallback(OpenApiCallback self)
        {
            Stop();
            return null;
        }

        public override object? VisitCallbacks(IDictionary<string, OpenApiCallback> self)
        {
            Stop();
            return null;
        }

        public override object? VisitComponents(OpenApiComponents self)
        {
            Stop();
            return null;
        }

        public override object? VisitContact(OpenApiContact self)
        {
            Stop();
            return null;
        }

        public override object? VisitEncoding(OpenApiEncoding self)
        {
            Stop();
            return null;
        }

        public override object? VisitEncodings(IDictionary<string, OpenApiEncoding> self)
        {
            Stop();
            return null;
        }

        public override object? VisitEnumPrimitive(IOpenApiPrimitive self)
        {
            Stop();
            return null;
        }

        public override object? VisitExample(OpenApiExample self)
        {
            Stop();
            return null;
        }

        public override object? VisitExamples(IDictionary<string, OpenApiExample> self)
        {
            Stop();
            return null;
        }

        public override object? VisitExtension(IOpenApiExtension self)
        {
            Stop();
            return null;
        }

        public override object? VisitExtensions(IDictionary<string, IOpenApiExtension> self)
        {
            Stop();
            return null;
        }

        public override object? VisitExternalDocs(OpenApiExternalDocs self)
        {
            Stop();
            return null;
        }

        public override object? VisitHeader(OpenApiHeader self)
        {
            Stop(); return null;
        }

        public override object? VisitHeaders(IDictionary<string, OpenApiHeader> self)
        {
            Stop();
            return null;
        }

        public override object? VisitInfo(OpenApiInfo self)
        {
            Stop();
            return null;
        }

        public override object? VisitLicense(OpenApiLicense self)
        {
            Stop();
            return null;
        }

        public override object? VisitLink(OpenApiLink self)
        {
            Stop();
            return null;
        }

        public override object? VisitLinks(IDictionary<string, OpenApiLink> self)
        {
            Stop();
            return null;
        }

        public override object? VisitMediaTypes(IDictionary<string, OpenApiMediaType> self)
        {
            Stop();
            return null;
        }

        public override object? VisitOperations(IDictionary<OperationType, OpenApiOperation> self)
        {
            Stop();
            return null;
        }

        public override object? VisitParameters(IDictionary<string, OpenApiParameter> self)
        {
            Stop();
            return null;
        }

        public override object? VisitPathItems(Dictionary<RuntimeExpression, OpenApiPathItem> self)
        {
            Stop();
            return null;
        }

        public override object? VisitReference(OpenApiReference self)
        {
            Stop();
            return null;
        }

        public override object? VisitRequestBodies(IDictionary<string, OpenApiRequestBody> self)
        {
            Stop();
            return null;
        }

        public override object? VisitResponses(IDictionary<string, OpenApiResponse> self)
        {
            Stop();
            return null;
        }

        public override object? VisitRuntimeExpression(RuntimeExpression self)
        {
            Stop();
            return null;
        }

        public override object? VisitRuntimeExpressionWrapper(RuntimeExpressionAnyWrapper self)
        {
            Stop();
            return null;
        }

        public override object? VisitRuntimeExpressionWrappers(Dictionary<string, RuntimeExpressionAnyWrapper> self)
        {
            Stop();
            return null;
        }

        public override object? VisitSchemas(IList<OpenApiSchema> self)
        {
            Stop();
            return null;
        }

        public override object? VisitSchemas(IDictionary<string, OpenApiSchema> self)
        {
            Stop();
            return null;
        }

        public override object? VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            Stop();
            return null;
        }

        public override object? VisitSecurityRequirements(IList<OpenApiSecurityRequirement> self)
        {
            Stop();
            return null;
        }

        public override object? VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            Stop();
            return null;
        }

        public override object? VisitSecurityScheme(KeyValuePair<OpenApiSecurityScheme, IList<string>> self)
        {
            Stop();
            return null;
        }

        public override object? VisitSecuritySchemes(IDictionary<string, OpenApiSecurityScheme> self)
        {
            Stop();
            return null;
        }

        public override object? VisitServer(OpenApiServer self)
        {
            Stop();
            return null;
        }

        public override object? VisitServers(IList<OpenApiServer> self)
        {
            Stop();
            return null;
        }

        public override object? VisitTag(OpenApiTag self)
        {
            Stop();
            return null;
        }

        public override object? VisitTags(IList<OpenApiTag> self)
        {
            Stop();
            return null;
        }

        public override object? VisitVariable(OpenApiServerVariable self)
        {
            Stop();
            return null;
        }

        public override object? VisitVariables(IDictionary<string, OpenApiServerVariable> self)
        {
            Stop();
            return null;
        }


        private OpenApiDocument _document;
        private readonly ContextGenerator _ctx;

    }


}