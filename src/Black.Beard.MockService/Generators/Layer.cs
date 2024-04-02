using Bb.Json.Jslt.CustomServices;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Diagnostics;

namespace Bb.OpenApiServices
{


    public class Layer
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer"/> class.
        /// </summary>
        public Layer()
        {
            this._layers = new List<Layer>();
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Layer> Children { get => _layers; }

        public string Name { get; internal set; }

        public string Type { get; internal set; }

        public IOpenApiElement Instance { get; internal set; }
        public string Source { get; internal set; }

        public void AddLayer(Layer layer)
        {
            _layers.Add(layer);
        }

        public override string ToString()
        {

            if (Type == "array")
                return $"{Type}<{_layers[0].ToString()}>";

            if (!string.IsNullOrEmpty(Name))
                return $"{Type} : {Name}";

            return $"{Type}";

        }


        public static bool EvaluateArray(ContextGenerator ctx, Layer left, Layer right)
        {
            bool result = false;
            for (int i = 0; i < right._layers.Count; i++)
            {

                var cLeft = left.Children[0];
                var cRight = right.Children[0];

                var same = cLeft.Type == cRight.Type;

                if (same && cLeft.Type == "array")
                {

                    Stop();
                    EvaluateArray(ctx, cLeft, cRight);

                }
                else if (same && cLeft.Type == "object")
                {
                    var r = EvaluateObject(ctx, cLeft, cRight);
                    if ( (r))
                    {
                        var datas = ctx.GetDataFor(right.Instance);
                        datas.SetData("source", "@" + left.Source);
                        result = true;
                    }

                }

                else
                {

                    Stop();

                }

            }

            return result;

        }

        public static bool EvaluateObject(ContextGenerator ctx, Layer left, Layer right)
        {

            for (int i = 0; i < right._layers.Count; i++)
            {

                var cLeft = left.Children[0];
                var cRight = right.Children.FirstOrDefault(c => c.Name == cLeft.Name);
                if (cRight != null)
                {
                    var datas = ctx.GetDataFor(cRight.Instance);
                    var path = ResolvePath(left.Instance);
                    var p = path.Split('/').Skip(1).ToList();
                    p.Insert(0, "$");
                    p.Add(cLeft.Name);
                    path = string.Join('.', p);

                    datas.SetData("path", path);
                    return true;

                }

            }

            return false;

        }

        private static string ResolvePath(IOpenApiElement instance)
        {

            if (instance is OpenApiSchema s)
            {
                if (s.Reference != null)
                    return string.Join('/', s.Reference.ReferenceV3.Split('/').Skip(3));

                else if (s.Items != null)
                {
                    Stop();
                    return ResolvePath(s.Items);
                }

                else if (s.Properties != null)
                {
                    Stop();

                }

            }
            else
            {
                Stop();
            }

            return null;

        }

        public void Evaluate(Layer right, ContextGenerator ctx)
        {

            if (this.Type == "array" && right.Type == "array")
            {
                EvaluateArray(ctx, this, right);
            }

            else if (this.Type == "object" && right.Type == "object")
            {
                Stop();
                EvaluateObject(ctx, this, right);
            }

        }



        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        protected static void Stop()
        {
            new StackTrace().GetFrame(1);
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        private List<Layer> _layers;

    }


}