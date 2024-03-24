using Bb.Extensions;
using Bb.OpenApiServices;
using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{


    public abstract class OpenApiHttpListenerBase 
        : DiagnosticGeneratorBase<HttpListenerBase>
        , IServiceGenerator<OpenApiDocument>
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiHttpListenerBase"/> class.
        /// </summary>
        public OpenApiHttpListenerBase()
        {

        }

        public void Parse(OpenApiDocument self, ContextGenerator ctx)
        {
            Initialize(ctx);
            this.Result = self.Accept(this);
        }


        public HttpListenerBase Result { get; private set; }

        protected OpenApiDocument _self;

    }
}
