using Bb.Extensions;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bb.Services.Chain
{
    public static class JsonOpenApiExtensions
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
                    JsonNode value = JsonNode.Parse(ConvertPath(current.Value.GetRawText()));
                    definitions.Add(name, value);
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
                target["maxItems"] = source.MaxItems.Value;

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
}
