
using Bb.Codings;
using Bb.Extensions;
using Bb.OpenApi;
using Bb.OpenApiServices;
using Bb.Services.Chain;
using Bb.Services.Managers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Diagnostics.Contracts;

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
        public ProjectBuilderContract(ProjectBuilderProvider parent, string contract)
        {
            Parent = parent;
            _logger = parent._logger;
            ContractName = contract;
            Root = parent.Root.Combine(contract);
        }

        /// <summary>
        /// Templates wants to know if exist.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public bool TemplateExistsOnDisk(string templateName)
        {
            return Directory.Exists(Root.Combine("Templates", templateName + ".json"));
        }

        /// <summary>
        /// return the list of template on the disk
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public IEnumerable<FileInfo> Templates()
        {
            return Root.Combine("Templates").AsDirectory().GetFiles("*.json").ToArray();
        }

        public FileInfo? ResolvePath(string filename)
        {
            return Root.Combine("Templates").AsDirectory().GetFiles(filename).FirstOrDefault();
        }


        /// <summary>
        /// Writes the document on disk.
        /// </summary>
        /// <param name="upFile">The uploaded file.</param>
        public ProjectBuilderContract WriteOnDisk(IFormFile upFile)
        {
            
            var target = Root.Combine("model" + Path.GetExtension(upFile.FileName))
                .AsFile();

            var file = upFile.Save(target, true);
            _templateFilename = file.FullName;

            var dir = Root.Combine("Templates").AsDirectory();
            if (dir.Exists)
                dir.Delete(true);

            return this;

        }

        /// <summary>
        /// Return the listener.
        /// </summary>
        /// <returns></returns>
        public HttpListenerBase Get()
        {

            if (_listener == null)
                _listener = Generate(new ContextGenerator(Root));

            return _listener;

        }

        /// <summary>
        /// Generate the model ans return the diagnostic
        /// </summary>
        /// <returns></returns>
        public ContextGenerator Generate()
        {
            var ctx = new ContextGenerator(Root);
            Generate(ctx);
            return ctx;
        }

        /// <summary>
        /// Generate the listener
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public HttpListenerBase Generate(ContextGenerator ctx)
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


            ctx = new ContextGenerator(Root);

            new ServiceGeneratorProcess<OpenApiDocument>(ctx)
            .Append(new OpenApiValidator(), new OpenApiGenerateDataTemplate())
            .Generate(_document);

            var generator = new OpenApiHttpListener();
            generator.Parse(_document, ctx);

            return generator.Result;

        }


        /// <summary>
        /// Removes the template if exists.
        /// </summary>
        public void RemoveTemplateIfExists()
        {
            var f = new FileInfo(_templateFilename);
            if (f.Exists)
                f.Delete();
        }


        /// <summary>
        /// The contract name
        /// </summary>
        public readonly string ContractName;

        /// <summary>
        /// The path root
        /// </summary>
        public readonly string Root;

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
        private HttpListenerBase _listener;
    }
}
