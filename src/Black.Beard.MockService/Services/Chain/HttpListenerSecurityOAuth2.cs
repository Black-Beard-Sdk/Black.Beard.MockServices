namespace Bb.Services.Chain
{
    public class HttpListenerSecurityOAuth2 : HttpListenerSecurity
    {


        public HttpListenerSecurityOAuth2(ApiKeyInEnum kind, string name) : base(kind, name)
        {

        }

        public override Task InvokeAsync(HttpListenerContext context)
        {
            Stop();
            return Task.CompletedTask;
        }

        public override object Clone()
        {
            var result = new HttpListenerSecurityOAuth2(this.Kind, this.Name);

            foreach (var item in Scopes)
                result.Scopes.Add(item);

            return result;

        }

    }


}
