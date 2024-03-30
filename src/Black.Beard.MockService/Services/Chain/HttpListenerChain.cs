namespace Bb.Services.Chain
{

    public abstract class HttpListenerChain : HttpListenerBase
    {

        public HttpListenerChain()
        {

        }
             
        public override void SetNext(HttpListenerBase next)
        {
            if (_next == null)
                _next = next;
            else
                _next.SetNext(next);
        }

        public override void Insert(HttpListenerBase next)
        {
            if (_next == null)
                _next = next;
            else
            {
                var o = _next;
                _next = next;
                _next.SetNext(o);
            }
        }

        internal override T Resolve<T>()
        {
        
            if (this is T a)
                return a;

            if (_next != null)
                return _next.Resolve<T>();

            return default;
        
        }

        protected HttpListenerBase Next => _next;

        private HttpListenerBase _next;

    }


}
