namespace Bb.Services.Chain
{
    public class HttpListenerSecurityOpenIdConnect : HttpListenerSecurity
    {


        public HttpListenerSecurityOpenIdConnect(ApiKeyInEnum kind, string name) : base(kind, name)
        {

        }

        public override Task InvokeAsync(HttpListenerContext context)
        {
            Stop();
            return Task.CompletedTask;

        }

        public override object Clone()
        {
            var result = new HttpListenerSecurityOpenIdConnect(this.Kind, this.Name);

            foreach (var item in Scopes)
                result.Scopes.Add(item);

            return result;
        }

    }


}
