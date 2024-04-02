
using Bb.Codings;
using Microsoft.OpenApi.Extensions;
using Bb.Extensions;
using Bb.OpenApi;
using Bb.OpenApiServices;
using Bb.Services.Chain;
using Bb.Services.Managers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Validations;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Bb.Services
{

    /// <summary>
    /// manage the contract in the referential
    /// </summary>
    public class ProjectBuilderContract
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBuilderContract"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="contract">The contract.</param>
        public ProjectBuilderContract(ProjectBuilderProvider parent, string contract, bool virtualContract)
        {

            Parent = parent;
            _logger = parent._logger;
            ContractName = contract;
            _virtual = virtualContract;

            if (!virtualContract)
            {
                Root = parent.Root.Combine(contract);
                _rootWww = Assembly.GetExecutingAssembly().Location.AsFile().Directory;
                _index = _rootWww.Combine("Generators", "_index.html").AsFile();
                _index.Refresh();
            }



        }

        /// <summary>
        /// Templates wants to know if exist.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public bool TemplateExistsOnDisk(string templateName)
        {
            if (_virtual)
                return false;
            return Directory.Exists(Root.Combine("Templates", templateName + ".json"));
        }

        /// <summary>
        /// return the list of template on the disk
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public IEnumerable<string>? Templates()
        {
            if (_virtual)
                return null;
            var dir = Root.Combine("Templates").AsDirectory();
            dir.Refresh();
            if (dir.Exists)
                return dir.GetFiles("*.json").Select(c => c.Name).ToArray();
            return null;
        }

        public FileInfo? ResolvePath(string filename)
        {
            if (_virtual)
                return null;
            return Root.Combine("Templates").AsDirectory().GetFiles(filename).FirstOrDefault();
        }


        /// <summary>
        /// Writes the document on disk.
        /// </summary>
        /// <param name="upFile">The uploaded file.</param>
        public ProjectBuilderContract WriteOnDisk(IFormFile upFile)
        {

            if (_virtual)
                return this;

            var target = Root.Combine("model" + Path.GetExtension(upFile.FileName))
                .AsFile();

            target.Refresh();
            if (target.Exists)
                target.Delete();

            var file = upFile.Save(target, true);
            _templateFilename = file.FullName;

            var dir = Root.Combine("Templates").AsDirectory();
            if (dir.Exists)
                dir.Delete(true);

            return this;

        }


        /// <summary>
        /// Set the listener.
        /// </summary>
        /// <param name="listener"></param>
        public void Set(HttpListeners listener)
        {
            _listener = listener;
        }


        /// <summary>
        /// Return the listener.
        /// </summary>
        /// <returns></returns>
        public HttpListenerBase? Get()
        {

            if (_listener == null)
            {
                var ctx = new ContextGenerator(Root);
                _listener = (HttpListeners)Generate(ref ctx);
            }
            
            return _listener;

        }


        /// <summary>
        /// Generate the model ans return the diagnostic
        /// </summary>
        /// <returns></returns>
        public ContextGenerator Generate()
        {
            var ctx = new ContextGenerator(Root);
            _listener = (HttpListeners)Generate(ref ctx);
            return ctx;
        }


        /// <summary>
        /// Generate the listener
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OpenApiDocument GetOpenApiContract()
        {

            if (string.IsNullOrEmpty(_templateFilename))
            {

                var p = Root
                    .AsDirectory()
                    .GetFiles("model.*")
                    .Where(c => c.Extension.ToLower() == ".json" || c.Extension.ToLower() == ".yml")
                    .FirstOrDefault();

                if (p != null)
                    _templateFilename = p.FullName;

            }

            var _document = _templateFilename.LoadOpenApiContract();
            return _document;

        }


        /// <summary>
        /// Generate the listener
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public HttpListenerBase? Generate(ref ContextGenerator ctx)
        {

            var _document = GetOpenApiContract();

            if (ctx == null)
                ctx = new ContextGenerator(Root)
                {
                 
                };


            var errors = _document.Validate(ValidationRuleSet.GetDefaultRuleSet());

            if (errors.Any())
            {
                foreach (var item in errors)
                    ctx.Diagnostics.AddError(new Analysis.DiagTraces.LocationPath(item.Pointer), "error", item.Message);
                return null;
            }

            new ServiceGeneratorProcess<OpenApiDocument>(ctx)
                .Append(new Bb.OpenApiServices.OpenApiValidator())
                .Generate(_document);

            if (ctx.Diagnostics.Success)
            {


                var a = new ModelAnalyze(ctx);
                a.VisitDocument(_document);


                new ServiceGeneratorProcess<OpenApiDocument>(ctx)
                    .Append(new OpenApiGenerateDataTemplate())
                    .Generate(_document);

                var generator = new OpenApiHttpListenerBuilder();
                generator.Parse(_document, ctx);


                if (_index.Exists)
                {
                    var content = _index.LoadFromFile()
                        .Replace("{{path}}", @".\" + Path.GetFileName(_templateFilename))

                        ;
                    var indexTarget = Root.Combine("index.html").AsFile();

                    if (indexTarget.Exists)
                        indexTarget.Delete();

                    indexTarget.FullName.Save(content);
                }

                return generator.Result;

            }
            else
            {
                
            }

            return null;

        }


        public void Clean()
        {
            Root.AsDirectory().Delete(true);
        }


        /// <summary>
        /// The contract name
        /// </summary>
        public readonly string ContractName;
        private readonly bool _virtual;

        /// <summary>
        /// The path root
        /// </summary>
        public readonly string Root;
        private readonly DirectoryInfo? _rootWww;
        private readonly FileInfo _index;

        /// <summary>
        /// Gets the parent that create the current class.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ProjectBuilderProvider Parent { get; }

        internal readonly ILogger<ProjectBuilderProvider> _logger;
        private volatile object _lock = new object();
        private string _templateFilename;
        private HttpListeners _listener;
    }
}
