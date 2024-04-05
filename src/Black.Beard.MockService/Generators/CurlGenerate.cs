using Bb.Asts;
using Bb.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Xml.Linq;

namespace Bb.OpenApiServices
{

    // https://reqbin.com/req/c-bjcj04uw/curl-send-cookies-example
    public class CurlGenerator : OpenApiGenericVisitor<string>
    {

        public CurlGenerator(ContextGenerator ctx, params string[] path)
        {
            this._ctx = ctx;
            this._rootPath = "http://{host}:{port}/" + path.Concat("/");
            this._writer = new Writer();
            this._globalssecurities = new HashSet<string>();
            _curls = new List<StringBuilder>();

        }

        public List<StringBuilder> Urls => _curls;

        public override string? VisitDocument(OpenApiDocument self)
        {
            using (var store = NewStore())
            {
                this._document = self;
                self.Components.SecuritySchemes.Accept(this);
                self.SecurityRequirements.Accept(this);
                self.Paths.Accept(this);
            }

            return null;
        }

        public override string? VisitPaths(OpenApiPaths self)
        {


            foreach (var path in self)
            {

                var s = _globalssecurities.ToList();
                if (s.Count == 0)
                    s.Add(string.Empty);

                foreach (var item in s)
                {

                    SecurityScheme security = null;
                    if (!string.IsNullOrEmpty(item))
                        security = _securities[item];

                    foreach (var operation in path.Value.Operations)
                    {

                        _writer.Clear();
                        _writer.Append("curl ");

                        try
                        {

                            _writer.Append("-X ");
                            _writer.Append(operation.Key.ToString().ToUpper());

                            WriteUrl(path, security);

                            if (security != null && !string.IsNullOrEmpty(security.Header))
                                _writer.Append(security.Header + " ");

                            if (security != null && !string.IsNullOrEmpty(security.Cookie))
                                _writer.Append(security.Cookie + " ");

                            operation.Accept(this);

                            _writer.TrimEnd();
                            _curls.Add(new StringBuilder(_writer.ToString()));

                        }
                        catch (Exception ex)
                        {
                            this._curls.Add(new StringBuilder($"Failed to generate curl {path.Key} -X {operation.Key.ToString()}"));
                        }

                    }

                }


            }

            return null;
        }

        private void WriteUrl(KeyValuePair<string, OpenApiPathItem> path, SecurityScheme security)
        {

            _writer.Append(" \"");

            //if (!string.IsNullOrEmpty(security.Path))
            //    _writer.Append(security.Path);

            _writer.Append(path.Key);

            List<string> query = new List<string>();
            if (security != null && !string.IsNullOrEmpty(security.Query))
                query.Add(security.Query);

            if (query.Count > 0)
            {
                _writer.Append("?");
                _writer.Append(query[0]);
                for (int i = 2; i < query.Count; i++)
                    _writer.Append("&" + query[i]);
            }

            _writer.Append("\" ");

        }

        public override string? VisitPathItem(OpenApiPathItem self)
        {
            return null;
        }


        public override string? VisitSecuritySchemes(IDictionary<string, OpenApiSecurityScheme> self)
        {

            var keys = self.Keys.ToList();
            var values = self.Values.ToList();

            for (var i = 0; i < keys.Count; i++)
            {
                using (var store = NewStore())
                {
                    var k = keys[i];
                    store.AddInStorage("security", k);
                    var v = values[i];
                    v.Accept(this);
                }

            }
            return null;
        }

        public override string? VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            this.TryGetInStorage("security", out string securityName);
            var s = new SecurityScheme()
            {
                Name = securityName,
                Type = self.Type,
            };

            switch (self.Type)
            {
                case SecuritySchemeType.ApiKey:

                    switch (self.In)
                    {
                        case ParameterLocation.Query:
                            s.Query = $"{Url.Encode(self.Name)}={{Your api key encoded in url}}'";
                            break;
                        case ParameterLocation.Header:
                            s.Header = $"-H '{self.Name}: {{Your api key}}'";
                            break;
                        case ParameterLocation.Path:
                            Stop();
                            break;
                        case ParameterLocation.Cookie:
                            s.Cookie = $"-b \"{self.Name}={{Your api key}}\"";
                            break;
                        default:
                            break;
                    }
                    break;
                case SecuritySchemeType.Http:

                    switch (self.Scheme.ToLower())
                    {
                        case "basic":
                            s.Header = "-u \"{user}:{password}\"";
                            break;

                        case "bearer":
                            s.Header = "-H \"Authorization: Bearer {jwt token}\"";
                            break;

                        default:
                            Stop();
                            break;
                    }
                    break;
                case SecuritySchemeType.OAuth2:
                    Stop();
                    break;

                case SecuritySchemeType.OpenIdConnect:
                    Stop();
                    break;
                default:
                    Stop();
                    break;
            }

            this._securities.Add(s.Name, s);

            return null;
        }

        public override string? VisitSecurityRequirements(IList<OpenApiSecurityRequirement> self)
        {
            foreach (var item in self)
                item.Accept(this);
            return null;
        }

        public override string? VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            foreach (var item in self)
                this._globalssecurities.Add(item.Key.Reference.Id);
            return null;
        }


        public override string? VisitOperation(OpenApiOperation self)
        {
            if (self.RequestBody != null)
                self.RequestBody.Accept(this);
            return null;
        }

        public override string? VisitRequestBody(OpenApiRequestBody self)
        {

            if (self.Content != null)
            {
                foreach (var item in self.Content)
                {

                    var mediaType = item.Key;
                    _writer.Append($"-H 'accept: {mediaType}' ");
                    var schema = item.Value.Schema;

                    if (schema != null)
                    {

                        string data = string.Empty;

                        if (schema.Type == "object")
                        {

                            if (schema.Reference != null && !string.IsNullOrEmpty(schema.Reference.Id))
                                data = schema.Reference.Id;

                            else
                                data = "{no describes}";

                        }
                        else if (schema.Type == "array")
                        {

                            if (schema.Items?.Reference?.Id != null
                                && !string.IsNullOrEmpty(schema.Items.Reference.Id))
                                data = schema.Items.Reference.Id;

                            else
                            {

                            }

                        }
                        else
                            data = "{no describes}";

                        _writer.Append($"-d '{{{data}}}' ");

                    }
                }
            }

            return null;

        }

        public override string? VisitParameter(OpenApiParameter self)
        {
            Stop();
            return null;
        }


        public override string? VisitCallback(OpenApiCallback self)
        {
            Stop();
            return null;
        }

        public override string? VisitCallbacks(IDictionary<string, OpenApiCallback> self)
        {
            Stop();
            return null;
        }

        public override string? VisitComponents(OpenApiComponents self)
        {
            Stop();
            return null;
        }

        public override string? VisitContact(OpenApiContact self)
        {
            Stop();
            return null;
        }

        public override string? VisitEncoding(OpenApiEncoding self)
        {
            Stop();
            return null;
        }

        public override string? VisitEncodings(IDictionary<string, OpenApiEncoding> self)
        {
            Stop();
            return null;
        }

        public override string? VisitEnumPrimitive(IOpenApiPrimitive self)
        {
            Stop();
            return null;
        }

        public override string? VisitExample(OpenApiExample self)
        {
            Stop();
            return null;
        }

        public override string? VisitExamples(IDictionary<string, OpenApiExample> self)
        {
            Stop();
            return null;
        }

        public override string? VisitExtension(IOpenApiExtension self)
        {
            Stop();
            return null;
        }

        public override string? VisitExtensions(IDictionary<string, IOpenApiExtension> self)
        {
            Stop();
            return null;
        }

        public override string? VisitExternalDocs(OpenApiExternalDocs self)
        {
            Stop();
            return null;
        }

        public override string? VisitHeader(OpenApiHeader self)
        {
            Stop();
            return null;
        }

        public override string? VisitHeaders(IDictionary<string, OpenApiHeader> self)
        {
            Stop();
            return null;
        }

        public override string? VisitInfo(OpenApiInfo self)
        {
            Stop();
            return null;
        }

        public override string? VisitLicense(OpenApiLicense self)
        {
            Stop();
            return null;
        }

        public override string? VisitLink(OpenApiLink self)
        {
            Stop();
            return null;
        }

        public override string? VisitLinks(IDictionary<string, OpenApiLink> self)
        {
            Stop();
            return null;
        }

        public override string? VisitMediaTypes(IDictionary<string, OpenApiMediaType> self)
        {
            Stop();
            return null;
        }

        public override string? VisitMediaType(OpenApiMediaType self)
        {
            Stop();
            return null;
        }

        public override string? VisitOperations(IDictionary<OperationType, OpenApiOperation> self)
        {
            Stop();
            return null;
        }

        public override string? VisitParameters(IDictionary<string, OpenApiParameter> self)
        {
            Stop();
            return null;
        }

        public override string? VisitParameters(IList<OpenApiParameter> self)
        {
            Stop();
            return null;
        }

        public override string? VisitPathItems(Dictionary<RuntimeExpression, OpenApiPathItem> self)
        {
            Stop();
            return null;
        }

        public override string? VisitReference(OpenApiReference self)
        {
            Stop();
            return null;
        }

        public override string? VisitRequestBodies(IDictionary<string, OpenApiRequestBody> self)
        {
            Stop();
            return null;
        }

        public override string? VisitResponse(OpenApiResponse self)
        {
            Stop();
            return null;
        }

        public override string? VisitResponses(IDictionary<string, OpenApiResponse> self)
        {
            Stop();
            return null;
        }

        public override string? VisitRuntimeExpression(RuntimeExpression self)
        {
            Stop();
            return null;
        }

        public override string? VisitRuntimeExpressionWrapper(RuntimeExpressionAnyWrapper self)
        {
            Stop();
            return null;
        }

        public override string? VisitRuntimeExpressionWrappers(Dictionary<string, RuntimeExpressionAnyWrapper> self)
        {
            Stop();
            return null;
        }

        public override string? VisitSchema(OpenApiSchema self)
        {
            Stop();
            return null;
        }

        public override string? VisitSchemas(IList<OpenApiSchema> self)
        {
            Stop();
            return null;
        }

        public override string? VisitSchemas(IDictionary<string, OpenApiSchema> self)
        {
            Stop();
            return null;
        }

        public override string? VisitSecurityScheme(KeyValuePair<OpenApiSecurityScheme, IList<string>> self)
        {
            Stop(); return null;
        }

        public override string? VisitServer(OpenApiServer self)
        {
            Stop(); return null;
        }

        public override string? VisitServers(IList<OpenApiServer> self)
        {
            Stop(); return null;
        }

        public override string? VisitTag(OpenApiTag self)
        {
            Stop(); return null;
        }

        public override string? VisitTags(IList<OpenApiTag> self)
        {
            Stop(); return null;
        }

        public override string? VisitVariable(OpenApiServerVariable self)
        {
            Stop(); return null;
        }

        public override string? VisitVariables(IDictionary<string, OpenApiServerVariable> self)
        {
            Stop(); return null;
        }

        private OpenApiDocument _document;
        private readonly ContextGenerator _ctx;
        private readonly string _rootPath;
        private readonly Writer _writer;
        Dictionary<string, SecurityScheme> _securities = new Dictionary<string, SecurityScheme>();
        private HashSet<string> _globalssecurities;
        private readonly List<StringBuilder> _curls;

        private class SecurityScheme
        {
            public string Header;
            public string Query;
            public string Cookie;
            public string Path;

            public SecuritySchemeType Type;

            public string? Name { get; internal set; }
        }

    }



    public class Writer
    {

        public Writer(SerializationStrategies strategy, StringBuilder? sb = null)
        {
            _sb = sb ?? new StringBuilder();
            this.Strategy = strategy;
            _index = 0;
        }

        public void Clear()
        {
            _sb.Clear();
        }

        public Writer(StringBuilder? sb = null)
        {
            _sb = sb ?? new StringBuilder();
            this.Strategy = new SerializationStrategies();
            _index = 0;
        }

        public void TrimBegin()
        {
            while (char.IsWhiteSpace(_sb[0]))
                _sb.Remove(0, 1);
        }

        public void TrimEnd()
        {
            if (_sb.Length > 0)
                while (char.IsWhiteSpace(_sb[_sb.Length - 1]))
                    _sb.Remove(_sb.Length - 1, 1);
        }

        public void TrimBegin(params char[] toFind)
        {
            while (toFind.Contains(_sb[0]))
                _sb.Remove(0, 1);
        }

        public void TrimEnd(params char[] toFind)
        {
            while (toFind.Contains(_sb[_sb.Length - 1]))
                _sb.Remove(_sb.Length - 1, 1);
        }

        public void EnsureEndBy(char txt)
        {
            if (!EndBy(txt))
                Append(txt);
        }

        public void EnsureEndBy(string txt)
        {
            if (!EndBy(txt))
                Append(txt);
        }

        public bool EndBy(string text)
        {
            if (_sb.Length >= text.Length)
            {
                var s = _sb.Length - text.Length;
                for (int i = 0; i < text.Length; i++)
                {
                    var left = _sb[s + i];
                    var right = text[i];
                    if (left != right)
                        return false;
                }
            }

            return true;

        }

        public bool EndBy(char text)
        {
            if (_sb.Length > 1)
                if (_sb[_sb.Length - 1] != text)
                    return false;
            return true;

        }

        public void CleanIndent()
        {
            while (_index > 0)
                DelIndent();
        }

        public void DelIndent()
        {
            _index--;
            if (_index < 0)
                _index = 0;
            else
            {
                var last = _sb[_sb.Length - 1];
                if (last == '\t')
                    _sb.Remove(_sb.Length - 1, 1);
            }
        }

        public void AddIndent()
        {

            if (_index < 0)
                _index = 0;

            _index++;

            _sb.Append('\t');

        }



        public void AppendEndLine(params object[] values)
        {
            foreach (var value in values)
                _sb.Append(value);
            AppendEndLine();
        }

        public void AppendEndLine()
        {

            _sb.AppendLine();
            for (int i = 0; i < _index; i++)
                _sb.Append('\t');

        }

        //public Writer Append(params object[] values)
        //{
        //    foreach (var value in values)
        //    {
        //        if (value is IWriter i)
        //            ToString(i);
        //        else
        //            _sb.Append(value);
        //    }
        //    return this;
        //}

        public Writer Append(params string[] values)
        {
            foreach (var value in values)
                _sb.Append(value);

            return this;
        }

        public Writer Append(params char[] values)
        {
            foreach (var value in values)
                _sb.Append(value);

            return this;
        }

        //public bool ToString(IWriter writer)
        //{

        //    if (writer != null)
        //    {

        //        if (!string.IsNullOrEmpty(writer.RuleName))
        //        {
        //            var strategy = this.Strategy.GetStrategy(writer.RuleName) ?? this.Strategy.DefaultStrategy;
        //            using (var blockStrategy = this.Apply(strategy))
        //            {
        //                return writer.ToString(this, strategy);
        //            }
        //        }


        //        return writer.ToString(this, this.Strategy.DefaultStrategy);
        //    }

        //    return false;

        //}

        public override string ToString()
        {
            return _sb.ToString();
        }

        public int Length => this._sb.Length;

        public StringBuilder Text { get => _sb; }

        public SerializationStrategies Strategy { get; }

        private readonly StringBuilder _sb;
        private int _index;

        private class _disposable : IDisposable
        {

            public _disposable(StrategySerializationItem strategy, Writer writer)
            {

                this._strategy = strategy;
                this._writer = writer;

                //_strategy.ApplyIndentLineBeforeStarting(_writer);
                //_strategy.ApplyReturnLineBeforeStarting(_writer);

            }

            protected virtual void Dispose(bool disposing)
            {

                if (!disposedValue)
                {
                    if (disposing)
                    {
                        //_strategy.ApplyIndentLineAfterEnding(_writer);
                        //_strategy.ApplyReturnLineAfterEnding(_writer);
                    }

                    disposedValue = true;
                }
            }

            private readonly StrategySerializationItem _strategy;


            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            private bool disposedValue;
            private Writer _writer;

        }

    }


}