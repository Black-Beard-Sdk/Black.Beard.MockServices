using Bb.Analysis.DiagTraces;
using Bb.Contracts;
using Bb.Jslt.Services;
using Oldtonsoft.Json.Linq;
using System.Text;

namespace Bb.Services.Managers
{
    public class ServiceProcessor
    {

        static ServiceProcessor()
        {

            //Bb.Jslt.Services.VariableResolver.Intercept = VariableInterceptor;

        }

        //private static void VariableInterceptor(IVariableResolver resolver, string key, bool status, object valueResolved)
        //{

        //}

        public ServiceProcessor(string? sourceDirectory = null)
        {

            // Initialization of the configuration
            var configuration = new TranformJsonAstConfiguration()
            {
                OutputPath = sourceDirectory ?? Environment.CurrentDirectory,
            };

            _templateprovider = new TemplateProvider(configuration);
            _templates = new Dictionary<string, JsltTemplate>();
            _templateDebugs = new Dictionary<string, JsltTemplate>();

        }

        public string GetDatas(string templateFile, bool withDebug, IDictionary<string, object> variables, ScriptDiagnostics diagnostics)
        {

            Oldtonsoft.Json.Linq.JToken result = default;

            // load mocked datas from file source
            // Create the sources object with the primary source of data and datas argument of the service
            //var source = SourceJson.GetFromFile(datas);

            var source = SourceJson.GetEmpty();
            var src = new Sources(source);
            if (variables != null)
            {

                foreach (var item in variables)
                {
                    var value = VariableConverterExtension.Convert(item.Value);
                    src.Variables.Add(item.Key, value);
                }
                
            }

            JsltTemplate template = GetTemplate(templateFile, withDebug, diagnostics);

            if (diagnostics.Success)
            {
                try
                {
                    RuntimeContext ctx = template.Transform(src);
                    var payload = ctx.TokenResult;
                    result = payload;
                }
                catch (Exception ex)
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
        private readonly TemplateProvider _templateprovider;
        private readonly Dictionary<string, JsltTemplate> _templates;
        private readonly Dictionary<string, JsltTemplate> _templateDebugs;
        private volatile object _lock = new object();
    }



    public static class VariableConverterExtension
    {


        static VariableConverterExtension()
        {
            _hvalues = new HashSet<Type>()
            {
                typeof(string),
                typeof(Uri),
                typeof(Guid),
                typeof(Byte),
                typeof(Byte[]),
                typeof(bool),
                typeof(double),
                typeof(float),
                typeof(long),
                typeof(ulong),
                typeof(int),
                typeof(uint),
                typeof(short),
                typeof(ushort),
                typeof(DateTime),
                typeof(TimeSpan),
            };
        }



        public static bool ConvertToJvalue(ref object value)
        {

            if (_hvalues.Contains(value.GetType()))
            {

                if (value is string s)
                {
                    var c = s[0];

                    if (c == '{' || c == '[')
                        value = JToken.Parse(s);
                    else
                        value = new JValue(value);
                    return true;
                }

                value = new JValue(value);
                return true;

            }

            return false;

        }



        public static object Convert(this object value)
        {

            if (value == null)
                return null;

            if (ConvertToJvalue(ref value))
                return value;

            if (value is JToken token)
            {

                if (value is JValue jv)
                {
                    var jc = jv.Value;
                    if (ConvertToJvalue(ref jc))
                        return jc;
                }

                if (value is JArray ja)
                {

                    var jc = new JArray();
                    foreach (var item in ja)
                    {
                        var i2 = item.Convert();
                        jc.Add(i2);
                    }

                    return jc;
                }

                var i = token.ToString();
                value = JToken.Parse(i);

                return value;

            }

            var vo = JToken.FromObject(value);
            return vo;

        }


        private static HashSet<Type> _hvalues;

    }


}
