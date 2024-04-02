using Bb.Extensions;
using Bb.OpenApi;
using Json.Schema;
using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{
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
