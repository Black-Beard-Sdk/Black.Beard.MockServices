
using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Specialized;
using System.Security.Claims;

namespace Bb.Services.Chain
{



    public class ApiKeyReferentiel
    {

        public ApiKeyReferentiel()
        {

        }


        public ApiKeyReferentiel Add(string apiKey, string username, params string[] roles)
        {
            var user = new user()
            {
                Name = username,
                Roles = roles
            };

            _items.Add(apiKey, user);
            return this;
        }



        internal bool TryGetValue(string? value, out ClaimsPrincipal user)
        {

            user = null;

            if (_items.TryGetValue(value, out var userValue))
            {
                user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, userValue.Name) }));
                return true;
            }

            return false;

        }


        private class user
        {
            public string Name { get; set; }
            public string[] Roles { get; set; }
        }

        private Dictionary<string, user> _items = new Dictionary<string, user>();

    }


    public class HttpListenerSecurityApiKeyAuth : HttpListenerSecurity
    {

        public HttpListenerSecurityApiKeyAuth(ApiKeyInEnum kind, string name) : base(kind, name)
        {


        }

        public override Task InvokeAsync(HttpListenerContext context)
        {

            bool result = false;
            string? value = null;

            switch (Kind)
            {

                case ApiKeyInEnum.Path:
                    Stop();
                    var array = context.Context.Request.Path.Value
                        .Trim('/')
                        .Split('/')
                        .ToArray()
                        .Skip(2)
                        .ToArray();

                    int Position = HttpListenerPathParameter.ResolvePosition(context.Context.Request.Path.Value, this.Name);
                    if (Position < array.Length)
                    {
                        var argument = array[Position];
                        context.AddArgument(this.Name, argument);
                    }
                    break;


                case ApiKeyInEnum.Cookie:
                    Stop();
                    result = context.Context.Request.Cookies.TryGetValue(this.Name, out value);
                    break;

                case ApiKeyInEnum.Header:
                    result = context.Context.Request.Headers.TryGetValue(this.Name, out var values);
                    if (result)
                    {
                        if (values.Count == 1)
                        {
                            var i = values[0];
                            value = i;
                        }
                        else
                        {
                            Stop();
                        }
                    }
                    break;

                case ApiKeyInEnum.Query:
                    Stop();
                    NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(context.Context.Request.QueryString.ToString());
                    result = queryString.AllKeys.Where(c => c == this.Name).Any();
                    if (result)
                        value = queryString[this.Name];

                    break;
                default:
                    break;
            }

            if (_referential.TryGetValue(value, out var user))
                context.Context.User = user;

            return Task.CompletedTask;

        }


        public void SetReferential(ApiKeyReferentiel referential)
        {
            this._referential = referential;
        }

        public override object Clone()
        {
            var result = new HttpListenerSecurityApiKeyAuth(this.Kind, this.Name);

            foreach (var item in Scopes)
                result.Scopes.Add(item);

            return result;
        }


        private ApiKeyReferentiel _referential;

    }


}
