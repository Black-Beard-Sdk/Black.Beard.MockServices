using Bb.Analysis.DiagTraces;
using Bb.Json.Jslt.Services;
using System.Text;

namespace Bb.Services.Managers
{
    public class ServiceProcessor
    {

        public ServiceProcessor(string? sourceDirectory = null)
        {

            // Initialization of the configuration
            var configuration = new TranformJsonAstConfiguration()
            {
                OutputPath = sourceDirectory ?? Environment.CurrentDirectory,
            };

            _templateprovider = new TemplateTransformProvider(configuration);
            _templates = new Dictionary<string, JsltTemplate>();
            _templateDebugs = new Dictionary<string, JsltTemplate>();

        }

        public string GetDatas(string templateFile, bool withDebug, IDictionary<string, Oldtonsoft.Json.Linq.JToken> variables, ScriptDiagnostics diagnostics)
        {

            Oldtonsoft.Json.Linq.JToken result = default;


            // load mocked datas from file source
            // Create the sources object with the primary source of data and datas argument of the service
            //var source = SourceJson.GetFromFile(datas);

            var source = SourceJson.GetEmpty();
            var src = new Sources(source);
            if (variables != null)
                src.Variables.Add(variables);

            JsltTemplate template = GetTemplate(templateFile, withDebug, diagnostics);

            if (diagnostics.Success)
            {
                try
                {
                    RuntimeContext ctx = template.Transform(src);
                    var payload = ctx.TokenResult;
                    result = payload;
                }
                catch (Exception)
                {
                    throw;
                }

                return result.ToString();
            }

            return null;

        }

        private JsltTemplate GetTemplate(string templateFile, bool withDebug, ScriptDiagnostics diagnostics)
        {

            var key = templateFile.LoadFromFile().CalculateCrc32().ToString();
            var _t = withDebug ? _templateDebugs : _templates;

            if (!_t.TryGetValue(templateFile, out JsltTemplate template))
                lock (_lock)
                    if (!_t.TryGetValue(key, out template))
                    {

                        //Build the template translator
                        var sbPayloadTemplate = new StringBuilder(templateFile.LoadFromFile());
                        template = _templateprovider.GetTemplate(sbPayloadTemplate, withDebug, templateFile, diagnostics);
                        if (diagnostics.Success)
                            _t.Add(key, template);

                    }
                     
            return template;
        }


        //private readonly Oldtonsoft.Json.JsonSerializer _serializer;
        private readonly TemplateTransformProvider _templateprovider;
        private readonly Dictionary<string, JsltTemplate> _templates;
        private readonly Dictionary<string, JsltTemplate> _templateDebugs;
        private volatile object _lock = new object();
    }

}
