using Bb.Codings;
using Bb.Json.Jslt.Asts;
using Microsoft.OpenApi.Models;
using Bb.Extensions;

namespace Bb.OpenApiServices
{

    public abstract class OpenApiGeneratorJsltBase : DiagnosticGeneratorBase<JsltBase>, IServiceGenerator<OpenApiDocument>
    {

        public OpenApiGeneratorJsltBase()
        {
            _tree = new DeclarationBloc();
            _code = new CodeBlock();
        }

        public void Parse(OpenApiDocument self, ContextGenerator ctx)
        {
            Initialize(ctx);
            self.Accept(this);
        }


        protected readonly DeclarationBloc _tree;
        protected OpenApiDocument _self;
        protected Data _datas = new Data();
        protected readonly CodeBlock _code;

    }

}