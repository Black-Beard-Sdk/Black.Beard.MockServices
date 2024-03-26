using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bb.Analysis.DiagTraces;
using Microsoft.AspNetCore.Mvc;
using Bb.Services.Managers;
using Bb.Curls;
using Bb.Servers.Exceptions;
using Bb.OpenApiServices;

namespace Bb.ParrotServices.Controllers
{

    [ApiController]
    [Route("[controller]")]
    // [Authorize(Policy = "Manager")]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(HttpExceptionModel))]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(HttpExceptionModel))]
    public class MockController : ControllerBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MockController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="server">The server.</param>
        public MockController(ILogger<MockController> logger, ProjectBuilderProvider builder)
        {
            _builder = builder;
            _logger = logger;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Consumes("application/text")]
        [HttpPost("Test")]
        //[RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Test([FromBody] string call)
        {
            CurlInterpreter curl = call;
            try
            {
                var result = await curl.CallToStringAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        /// <summary>
        /// Uploads the data source contract on server and generate the project for the specified contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="upfile">The file to upload that contains the contract in open api 3.*.</param>
        /// <returns>Return the list of template.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string[]))]
        [HttpPost("{contract}/upload")]
        //[Consumes("form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(100_000_000)]
        //[DisableRequestSizeLimit]
        public async Task<IActionResult> UploadOpenApiContract([FromRoute] string contract, IFormFile upfile)
        {

            _logger.LogDebug("Upload root : {root}", _builder.Root);

            // verify fileInfo
            if (string.IsNullOrEmpty(upfile?.FileName))
            {
                _logger.LogError("No file was received");
                throw new BadRequestException("No file was received");
            }

            if (upfile == null || upfile.Length == 0)
            {
                _logger.LogError("file stream is not selected or empty");
                return BadRequest("file stream is not selected or empty");
            }


            var _contract = _builder.Contract(contract, true);
            var ctx = _contract.WriteOnDisk(upfile)
                               .Generate()
                               ;

            var items = _builder.GetItems();

            if (ctx.Diagnostics.Success)
            {
                _logger.LogInformation($"{contract} has been generated");
                var templates = _contract.Templates().Select(c => c.Name).ToList();
                return Ok(GetModel(ctx.Diagnostics, templates));
            }
            else
            {
                _logger.LogInformation($"{contract} has been failed");
                return BadRequest(GetModel(ctx.Diagnostics, null));
            }

        }

        /// <summary>
        /// Download the specified data template.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="filename">the filename that you want download..</param>
        /// <returns></returns>
        /// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{contract}/download_template")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadDataTemplate([FromRoute] string contract, [FromQuery] string filename)
        {

            _logger.LogDebug("Download root : {root}", _builder.Root);

            var _contract = _builder.Contract(contract, true);

            var path = _contract.ResolvePath(filename);

            if (path != null && path.Length >= 1)
                return File(System.IO.File.OpenRead(path.FullName), "application/octet-stream", System.IO.Path.GetFileName(filename));

            return NotFound();

        }


        /// <summary>
        /// Uploads the data template and replace the existing file. The template is not tested.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="upfile">The file to replace.</param>
        /// <returns></returns>
        /// <exception cref="T:BadRequestObjectResult">No file received</exception>
        /// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{contract}/upload_template")]
        public async Task<IActionResult> UploadDataTemplate([FromRoute] string contract, IFormFile upfile)
        {


            _logger.LogDebug("upload root : {root}", _builder.Root);

            var _contract = _builder.Contract(contract, true);

            var path = _contract.ResolvePath(upfile.FileName);

            if (path != null)
            {

                path.Delete();
                path.Refresh();

                using (var stream = new FileStream(path.FullName, FileMode.CreateNew))
                {
                    await upfile.CopyToAsync(stream);
                }

                return Ok();
            }

            return NotFound(upfile.FileName);

        }



        private ContractModel GetModel(ScriptDiagnostics diagnostics, IEnumerable<string> templates)
        {

            var model = new ContractModel();

            foreach (var diagnostic in diagnostics)
                model.Diagnostic.Add(diagnostic.ToString());

            if (templates != null)
                foreach (var template in templates)
                    model.Templates.Add(template);

            return model;

        }

        internal readonly ProjectBuilderProvider _builder;
        private readonly ILogger<MockController> _logger;

    }




    public class ContractModel
    {

        public ContractModel()
        {
            this.Diagnostic = new List<string>();
            this.Templates = new List<string>();
        }

        public List<string> Diagnostic { get; set; }
        public List<string> Templates { get; set; }
    }


}
