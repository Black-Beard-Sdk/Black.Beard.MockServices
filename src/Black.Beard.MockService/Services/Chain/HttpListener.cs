namespace Bb.Services.Chain
{



    public class HttpListener : HttpListenerBase
    {


        public HttpListener(string path) : base()
        {
            this.PathToString = path;
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

        public override void Insert(HttpListenerBase next)
        {
            this._items.Add(next);
        }

        /// <summary>
        /// Evaluate if the request path is matched
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        internal IEnumerable<HttpListenerBase> Match(string[] fullPath, HttpListenerContext context)
        {

            List<HttpListenerBase> result = new List<HttpListenerBase>();

            if (fullPath.Length > this.Path.Length)
                return result;

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
                    {
                        context.Diagnostic($"Path : {this.PathToString} no match");
                        return result;
                    }
                }
            }

            context.Diagnostic($"Path : {this.PathToString} is matched");

            string methodType = context.Context.Request.Method.ToUpper();

            List<string> methods = new List<string>();
            foreach (var item in _items)
            {
                HttpListenerMethodBase method = item.Resolve<HttpListenerMethodBase>();
                if (method != null)
                {
                    var m = method.Type.ToString().ToUpper();
                    if (m == methodType)
                        result.Add(item);
                    else
                        methods.Add(m);
                }
            }

            if (result.Count == 0)
                context.Diagnostic($"Path : {this.PathToString} no method is matched {methodType} with available methods : {string.Join(", ", methods)}");

            return result;

        }

        public string PathToString { get; }
        public string[] Path { get; }

        private readonly List<HttpListenerBase> _items;

    }


}
