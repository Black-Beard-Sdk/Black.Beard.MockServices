using System.Diagnostics;

namespace Bb.Services.Chain
{

    public abstract class HttpListenerBase
    {

        public HttpListenerBase()
        {

        }

        public abstract Task InvokeAsync(HttpListenerContext context);

        public virtual void SetNext(HttpListenerBase next)
        {
          
        }


        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        protected void Stop()
        {
            new StackTrace().GetFrame(1);
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        internal virtual T Resolve<T>()
        {
            throw new NotImplementedException();
        }

    }


}
