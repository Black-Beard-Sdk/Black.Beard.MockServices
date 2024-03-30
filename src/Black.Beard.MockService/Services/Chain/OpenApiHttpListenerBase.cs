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
            this._storings = new Stack<IStore>();
        }

        public void Parse(OpenApiDocument self, ContextGenerator ctx)
        {
            this._document = self;
            Initialize(ctx);
            this.Result = self.Accept(this);
        }


        protected void Store(string key, object value)
        {
            _storings.Peek().AddInStorage(key, value);
        }

        protected IStore Store()
        {
            return new _disposeStoringClass(this);
        }

        private class _disposeStoringClass : IStore
        {

            public _disposeStoringClass(OpenApiHttpListenerBase document)
            {
                _document = document;
                this._dic = new Dictionary<string, object>();
                _document._storings.Push(this);
            }


            public void AddInStorage(string key, object value)
            {
                if (_dic.ContainsKey(key))
                    _dic[key] = value;
                else
                    _dic.Add(key, value);
            }

            public bool TryGetInStorage(string key, out object value)
            {
                return _dic.TryGetValue(key, out value);
            }

            public bool ContainsInStorage(string key)
            {
                return _dic.ContainsKey(key);
            }


            public void Dispose()
            {
                _document._storings.Pop();
            }

            private readonly OpenApiHttpListenerBase _document;
            private readonly Dictionary<string, object> _dic;
        }


        private Stack<IStore> _storings = new Stack<IStore>();

        public HttpListenerBase Result { get; private set; }

        protected OpenApiDocument _document;


    }

    public interface IStore : IDisposable
    {

        void AddInStorage(string key, object value);


        bool TryGetInStorage(string key, out object value);


        bool ContainsInStorage(string key);       

    }


}
