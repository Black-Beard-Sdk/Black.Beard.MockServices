namespace Bb.Services.Chain
{

    public abstract class HttpListenerParameter : HttpListenerChain
    {

        public HttpListenerParameter(string name, bool required) : base()
        {
            this.Name = name;
            this.Required = required;
            
        }

        public string Name { get; }

        public bool Required { get; }
        
    }


}
