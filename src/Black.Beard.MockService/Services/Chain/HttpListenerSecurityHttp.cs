namespace Bb.Services.Chain
{
    public class HttpListenerSecurityHttp : HttpListenerSecurity
    {


        public HttpListenerSecurityHttp(ApiKeyInEnum kind, string name) : base(kind, name)
        {

        }

        public override Task InvokeAsync(HttpListenerContext context)
        {
            Stop();
            return Task.CompletedTask;
        }

        public override  object Clone()
        {
            var result = new HttpListenerSecurityHttp(this.Kind, this.Name);

            foreach (var item in Scopes)
                result.Scopes.Add(item);

            return result;

        }             

    }


}
