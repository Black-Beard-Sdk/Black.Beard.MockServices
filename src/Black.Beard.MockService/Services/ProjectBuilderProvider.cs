using Bb.ComponentModel.Attributes;
using Bb.ComponentModel.Factories;
using Bb.Servers.Web.Models;
using System.Reflection;
using System.Text;

namespace Bb.Services.Managers
{




    [ExposeClass(Context = Constants.Models.Service, LifeCycle = IocScopeEnum.Singleton)]
    public class ProjectBuilderProvider : IInitialize
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBuilderProvider"/> class.
        /// </summary>
        /// <param name="host">The host centralize all ran service.</param>
        /// <param name="referential">The referential.</param>
        /// <param name="logger">The logger.</param>
        public ProjectBuilderProvider(ILogger<ProjectBuilderProvider> logger)
        {
            _logger = logger;
            _items = new Dictionary<string, ProjectBuilderContract>();
        }


        /// <summary>
        /// Initializes the <see cref="ProjectBuilderProvider" />.
        /// </summary>
        /// <param name="services">The service provider.</param>
        public virtual void Initialize(IServiceProvider services)
        {

            // services.GetKeyedService<Option<Configuration>>(Constants.Models.Configuration);

            Initialize(GlobalConfiguration.CurrentDirectoryToWriteGenerators);
        
        }


        /// <summary>
        /// Initializes the <see cref="ProjectBuilderProvider" />.
        /// </summary>
        /// <param name="pathRoot">The path root where the contracts will be generate.</param>
        public virtual void Initialize(string pathRoot)
        {
            _root = pathRoot;

            var rootWww = Assembly.GetExecutingAssembly().Location.AsFile().Directory;
            var redoc = rootWww.Combine("www", "scripts", "redoc.standalone.js").AsFile();
            if (!redoc.Exists)
            redoc.CopyToDirectory(_root);

        }



        /// <summary>
        /// return the contract for specified contract name.
        /// </summary>
        /// <param name="contractName">The contract name.</param>
        /// <param name="createIfNotExists">if set to <c>true</c> [create if not exists].</param>
        /// <returns></returns>
        public ProjectBuilderContract Contract(string contractName, bool createIfNotExists = false)
        {

            var r = Root.Combine(contractName).AsDirectory();
            if (r.Exists)
                createIfNotExists = true;

            contractName = contractName.ToLower();

            if (!_items.TryGetValue(contractName, out var builder))
                lock (_lock)
                    if (!_items.TryGetValue(contractName, out builder))
                        if (createIfNotExists)
                            _items.Add(contractName, builder = new ProjectBuilderContract(this, contractName, false));

            return builder;

        }

        /// <summary>
        /// return the contract for specified contract name.
        /// </summary>
        /// <param name="contractName">The contract name.</param>
        /// <param name="createIfNotExists">if set to <c>true</c> [create if not exists].</param>
        /// <returns></returns>
        public ProjectBuilderContract VirtualContract(string contractName)
        {

            contractName = contractName.ToLower();

            if (!_items.TryGetValue(contractName, out var builder))
                lock (_lock)
                    if (!_items.TryGetValue(contractName, out builder))
                            _items.Add(contractName, builder = new ProjectBuilderContract(this, contractName, true));

            return builder;

        }


        public void RegenerateIndex()
        {

            StringBuilder sb = new StringBuilder();

            var _rootWww = Assembly.GetExecutingAssembly().Location.AsFile().Directory;
            var _index = _rootWww.Combine("Generators", "Index.html").AsFile();
            foreach (var item in GetItems())
                sb.AppendLine(item);

            var content = _index.LoadFromFile().Replace("{{items}}", sb.ToString());

            var indexTarget = Root.Combine("index.html").AsFile();

            if (indexTarget.Exists)
                indexTarget.Delete();

            indexTarget.FullName.Save(content);

        }

        /// <summary>
        /// Return the list of Html contract index.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetItems()
        {
            List<string> _items = new List<string>();
            var r = Root.AsDirectory();
            foreach (var item in r.GetDirectories())
            {
                var c = item.GetFiles("index.html").FirstOrDefault();
                if (c != null)
                {
                    if (c.Exists)
                    {
                        var path = "/" + Path.Combine("www", c.FullName.Substring(Root.Length + 1)).Replace("\\", "/");
                        var t = $"<div><a href=\"{path}\">{item.Name}</a> </div>";
                        _items.Add(t);
                    }
                }
                else
                {
                    item.Delete(true);
                }
            }
            return _items;
        }


        /// <summary>
        /// Gets a value indicating whether contract exists in the referential.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contract exists]; otherwise, <c>false</c>.
        /// </value>
        public bool ContractExists
        {
            get
            {
                return Directory.Exists(_root)
                    && File.Exists(_root.Combine("contract.json"))
                    ;
            }
        }


        #region generators

        internal Type? ResolveGenerator(string template)
        {

            if (_generators != null)
                if (_generators.TryGetValue(template, out var generator))
                    return generator;

            return null;

        }


        #endregion generators


        /// <summary>
        /// Gets the root path for access to the resource.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        public string Root => _root;

        internal readonly ILogger<ProjectBuilderProvider> _logger;
        private readonly Dictionary<string, ProjectBuilderContract> _items;
        private string _root;
        private static Dictionary<string, Type>? _generators;
        private volatile object _lock = new object();

    }

}
