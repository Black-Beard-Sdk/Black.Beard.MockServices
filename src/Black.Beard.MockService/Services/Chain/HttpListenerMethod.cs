using Microsoft.OpenApi.Models;

namespace Bb.Services.Chain
{
    public abstract class HttpListenerMethod : HttpListenerChain
    {

        public HttpListenerMethod(OperationType type, string path, string templateFileName) : base()
        {
            this.Type = type;
            this.Path = path.Split('/');
            TemplateFileName = templateFileName;
        }

        public OperationType Type { get; }

        public string[] Path { get; }
        
        public string TemplateFileName { get; }

    }


}
