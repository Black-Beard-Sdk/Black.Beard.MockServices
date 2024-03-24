namespace Bb.Services.Chain
{
    public class HttpListener : HttpListenerBase
    {


        public HttpListener(string path) : base()
        {
            this.Path = path.Split('/');
            this._items = new List<HttpListenerBase>();
        }


        public override async Task InvokeAsync(HttpListenerContext context)
        {
            Stop();
            throw new NotImplementedException();
        }


        public override void SetNext(HttpListenerBase next)
        {
            this._items.Add(next);
        }

        /// <summary>
        /// Evaluate if the request path is matched
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        internal IEnumerable<HttpListenerBase> Match(string[] fullPath, string methodType)
        {

            if (fullPath.Length > this.Path.Length)
                yield  break;

            for (int i = 0; i < fullPath.Length; i++)
            {
                var currentItem = fullPath[i];
                var patternItem = this.Path[i];
                if (patternItem != currentItem)
                {
                    if (patternItem.StartsWith("{") && patternItem.EndsWith("}"))
                    {
                        // is a variable
                    }
                    else
                        yield break;
                }
            }

            foreach (var item in _items)
            {
                HttpListenerMethod method = item.Resolve<HttpListenerMethod>();
                if (method.Type.ToString().ToUpper() == methodType)
                    yield return item;

            }

            yield break;
        }

        public string[] Path { get; }

        private readonly List<HttpListenerBase> _items;

    }


}
