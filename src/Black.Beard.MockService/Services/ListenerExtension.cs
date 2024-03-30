using Bb.Services.Chain;
using Json.Schema;
using Microsoft.OpenApi.Models;

namespace Bb.Services
{



    public static class ListenerExtension
    {


        /// <summary>
        /// Add a new path in the server
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">path of web method</param>
        /// <param name="action">action to configure the new method</param>
        /// <returns></returns>
        public static HttpListeners AddPath(this HttpListeners self,
            string path, Action<HttpListener> action)
        {
            HttpListener result = new HttpListener(path.Trim('/'));
            action(result);
            self.SetNext(result);
            return self;
        }

        #region Methods

        /// <summary>
        /// Add a new method for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="method">method Type</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddOperation(this HttpListener self,
            OperationType method, Func<HttpListenerContext, Task> @delegate)
        {
            var meth = new HttpListenerMethodDelegate(method, self.PathToString, @delegate);
            self.SetNext(meth);
            return self;
        }

        public static HttpListener AddOperation(this HttpListener self, OperationType method, Action<HttpListenerMethodDelegate> action)
        {
            var meth = new HttpListenerMethodDelegate(method, self.PathToString);
            action(meth);
            self.SetNext(meth);
            return self;
        }

        /// <summary>
        /// Add a new method for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddPost(this HttpListener self, Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Post, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method Post for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddPost(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Post, action);
            return self;
        }

        /// <summary>
        /// Add a new method get for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddGet(this HttpListener self,
            Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Get, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method get for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddGet(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Get, action);
            return self;
        }

        /// <summary>
        /// Add a new method put for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddPut(this HttpListener self,
             Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Put, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method put for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddPut(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Put, action);
            return self;
        }

        /// <summary>
        /// Add a new method for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddOptions(this HttpListener self,
            Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Options, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method options for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddOptions(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Options, action);
            return self;
        }

        /// <summary>
        /// Add a new method for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddPatch(this HttpListener self,
            Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Patch, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method patch for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddPatch(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Patch, action);
            return self;
        }

        /// <summary>
        /// Add a new method delete for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddDelete(this HttpListener self,
           Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Delete, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method delete for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddDelete(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Delete, action);
            return self;
        }

        /// <summary>
        /// Add a new method for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddHead(this HttpListener self,
           Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Head, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method head for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddHead(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Head, action);
            return self;
        }

        /// <summary>
        /// Add a new method for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="delegate">method to execute</param>
        /// <returns></returns>
        public static HttpListener AddTrace(this HttpListener self,
           Func<HttpListenerContext, Task> @delegate)
        {
            AddOperation(self, OperationType.Trace, @delegate);
            return self;
        }

        /// <summary>
        /// Add a new method trace for the path
        /// </summary>
        /// <param name="self">Listener you want to add method</param>
        /// <param name="action">method to configure</param>
        /// <returns></returns>
        public static HttpListener AddTrace(this HttpListener self, Action<HttpListenerMethodDelegate> action)
        {
            AddOperation(self, OperationType.Trace, action);
            return self;
        }

        #endregion Methods

        #region Securities

        public static T SetSecurityApiKey<T>(this T self, ApiKeyInEnum kind, string name, Action<HttpListenerSecurityApiKeyAuth> action)
            where T : HttpListenerMethodBase
        {

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var api = new HttpListenerSecurityApiKeyAuth(kind, name);
            action(api);
            self.SetSecurity(api);
            return self;
        }

        public static T SetSecurityHttp<T>(this T self, ApiKeyInEnum kind, string name, Action<HttpListenerSecurityHttp> action)
            where T : HttpListenerMethodBase
        {

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var api = new HttpListenerSecurityHttp(kind, name);
            action(api);
            self.SetSecurity(api);
            return self;
        }

        public static T SetSecurityOAuth2<T>(this T self, ApiKeyInEnum kind, string name, Action<HttpListenerSecurityOAuth2> action)
            where T : HttpListenerMethodBase
        {

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var api = new HttpListenerSecurityOAuth2(kind, name);
            action(api);
            self.SetSecurity(api);
            return self;
        }

        public static T SetSecurityOpenIdConnect<T>(this T self, ApiKeyInEnum kind, string name, Action<HttpListenerSecurityOpenIdConnect> action)
            where T : HttpListenerMethodBase
        {

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var api = new HttpListenerSecurityOpenIdConnect(kind, name);
            action(api);
            self.SetSecurity(api);
            return self;
        }

        #endregion Securities

        #region parameters

        /// <summary>
        /// Add body parameter
        /// </summary>
        /// <param name="self">parent</param>
        /// <returns></returns>
        public static T AddParameterBody<T>(this T self)
            where T : HttpListenerMethodBase
        {
            self.AddParameter(new HttpListenerBodyParameter(true, null, null));
            return self;
        }

        /// <summary>
        /// Add body parameter
        /// </summary>
        /// <param name="self">parent</param>
        /// <param name="type">use a type for deserializes.</param>
        /// <returns></returns>
        public static T AddParameterBody<T>(this T self, Type? type = null)
            where T : HttpListenerMethodBase
        {
            self.AddParameter(new HttpListenerBodyParameter(true, type, null));
            return self;
        }

        /// <summary>
        /// Add body parameter
        /// </summary>
        /// <param name="self">parent</param>
        /// <param name="schema">specify a schema for validate the contract</param>
        /// <returns></returns>
        public static T AddParameterBody<T>(this T self, JsonSchema schema = null)
            where T : HttpListenerMethodBase
        {
            self.AddParameter(new HttpListenerBodyParameter(true, null, schema));
            return self;
        }

        /// <summary>
        /// Add parameter
        /// </summary>
        /// <param name="self">parent</param>
        /// <param name="kind">Kind of parameter</param>
        /// <param name="name">name of the parameter</param>
        /// <param name="required">the parameter is required</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T AddParameter<T>(this T self, ParameterKind kind, string name, bool required = false)
            where T : HttpListenerMethodBase
        {

            switch (kind)
            {
                case ParameterKind.Header:
                    self.SetNext(new HttpListenerHeaderParameter(name, required));
                    break;

                case ParameterKind.Path:
                    var position = HttpListenerPathParameter.ResolvePosition(self.PathToString, name);
                    if (position == -1)
                        throw new KeyNotFoundException(name);
                    self.SetNext(new HttpListenerPathParameter(name, position));
                    break;

                case ParameterKind.Cookies:
                    self.SetNext(new HttpListenerCookieParameter(name, required));
                    break;

                case ParameterKind.Query:
                    self.SetNext(new HttpListenerQueryParameter(name, required));
                    break;

                default:
                    throw new NotImplementedException(kind.ToString());

            }

            return self;

        }

        /// <summary>
        /// Add parameter like Cookie
        /// </summary>
        /// <param name="self">parent</param>
        /// <param name="name">name of the parameter</param>
        /// <param name="required">the parameter is required</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T AddParameterCookie<T>(this T self, string name, bool required = false)
            where T : HttpListenerMethodBase
        {
            self.SetNext(new HttpListenerCookieParameter(name, required));
            return self;

        }

        /// <summary>
        /// Add parameter like header
        /// </summary>
        /// <param name="self">parent</param>
        /// <param name="name">name of the parameter</param>
        /// <param name="required">the parameter is required</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T AddParameterHeader<T>(this T self, string name, bool required = false)
            where T : HttpListenerMethodBase
        {
            self.SetNext(new HttpListenerHeaderParameter(name, required));
            return self;

        }

        /// <summary>
        /// Add parameter like Query
        /// </summary>
        /// <param name="self">parent</param>
        /// <param name="name">name of the parameter</param>
        /// <param name="required">the parameter is required</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T AddParameterQuery<T>(this T self, string name, bool required = false)
            where T : HttpListenerMethodBase
        {
            self.SetNext(new HttpListenerQueryParameter(name, required));
            return self;

        }

        /// <summary>
        /// Add parameter in path
        /// </summary>
        /// <param name="self">parent</param>
        /// <param name="name">name of the parameter</param>
        /// <param name="required">the parameter is required</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T AddParameterPath<T>(this T self, string name)
            where T : HttpListenerMethodBase
        {
            var position = HttpListenerPathParameter.ResolvePosition(self.PathToString, name);
            if (position == -1)
                throw new KeyNotFoundException(name);
            self.SetNext(new HttpListenerPathParameter(name, position));
            return self;

        }

        #endregion parameters

    }

    public enum ParameterKind
    {
        Header,
        Path,
        Cookies,
        Query,
    }

}
