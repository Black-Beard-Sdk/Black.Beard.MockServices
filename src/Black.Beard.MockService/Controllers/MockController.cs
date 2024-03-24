using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bb.Analysis.DiagTraces;
using Microsoft.AspNetCore.Mvc;
using Bb.Services.Managers;
using Bb.Curls;
using Bb.Servers.Exceptions;

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
        //[Consumes("text")]
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

            // "curl -X GET \"https://localhost:49805/proxy/parcel/ParcelTracking/11111\" -H \"accept: application/json\""
        }


        /// <summary>
        /// Uploads the data source contract on server and generate the project for the specified contract.
        /// </summary>
        /// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        /// <param name="contract">The unique contract name.</param>
        /// <param name="upfile">The file to upload that contains the contract in open api 3.*.</param>
        /// <returns>Return the list of template.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ModelStateDictionary))]
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



            if (ctx.Diagnostics.Success)
            {
                _logger.LogInformation($"{contract} has been generated");
                return Ok(GetModel(ctx.Diagnostics));
            }
            else
            {
                _logger.LogInformation($"{contract} has been failed");
                return BadRequest(GetModel(ctx.Diagnostics));
            }


            //// Generate project
            //var result = templateObject.GenerateProject();

            //if (result != null && result.Context != null)
            //{
            //    if (result.Context.Diagnostics.Success)
            //    {
            //        _logger.LogInformation($"{template}/{contract} has been generated");
            //        return Ok(result);
            //    }
            //    else
            //        _logger.LogError($"Failed to generate {template}/{contract}");

            //    return BadRequest(GetBadModel(result.Context.Diagnostics));
            //}

            //var diag = new ScriptDiagnostics();
            //diag.AddError(TextLocation.Empty, "Project generation failed", "Project generation failed");

            //return BadRequest(GetBadModel(diag));

            return Ok();

        }


        ///// <summary>
        ///// Download the specified data template.
        ///// </summary>
        ///// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        ///// <param name="contract">The unique contract name.</param>
        ///// <param name="filename">the filename that you want download..</param>
        ///// <returns></returns>
        ///// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpGet("{contract}/download_template")]
        //[Produces("application/octet-stream")]
        //public async Task<IActionResult> DownloadDataTemplate([FromRoute] string template, [FromRoute] string contract, [FromQuery] string filename)
        //{

        //    var project = _builder.Contract(contract);

        //    ProjectBuilderTemplate templateObject;
        //    try
        //    {
        //        templateObject = project.Template(template);
        //    }
        //    catch (MockHttpException e)
        //    {
        //        _logger.LogError(e, e.Message);
        //        return NotFound(e.Message);
        //    }

        //    var dir = templateObject.GetDirectoryProject("Templates");
        //    var files = templateObject.GetFiles(dir, filename);
        //    if (files.Length == 1)
        //    {
        //        return File(System.IO.File.OpenRead(files[0].FullName), "application/octet-stream", System.IO.Path.GetFileName(filename));
        //    }
        //    return NotFound();

        //}


        ///// <summary>
        ///// Uploads the data template and replace the existing file. The template is not tested.
        ///// </summary>
        ///// <param name="template">template name of generation. If you don"t know. use 'mock'</param>
        ///// <param name="contract">The unique contract name.</param>
        ///// <param name="upfile">The file to replace.</param>
        ///// <returns></returns>
        ///// <exception cref="T:BadRequestObjectResult">No file received</exception>
        ///// <exception cref="T:NotFoundObjectResult">the template is not found</exception>
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpPost("{contract}/upload_template")]
        //public async Task<IActionResult> UploadDataTemplate([FromRoute] string template, [FromRoute] string contract, IFormFile upfile)
        //{

        //    // verify fileInfo
        //    if (string.IsNullOrEmpty(upfile?.FileName))
        //    {
        //        _logger.LogError("No file was received");
        //        throw new BadRequestException("No file was received");
        //    }

        //    if (upfile == null || upfile.Length == 0)
        //    {
        //        _logger.LogError("file stream is not selected or empty");
        //        return BadRequest("file stream is not selected or empty");
        //    }

        //    var project = _builder.Contract(contract);
        //    if (project == null)
        //        return NotFound(contract + " not found");

        //    ProjectBuilderTemplate? templateObject = project.Template(template);
        //    if (templateObject == null)
        //        return NotFound(template + " not found");

        //    var dir = templateObject.GetDirectoryProject("Templates");
        //    var files = templateObject.GetFiles(dir, upfile.FileName);

        //    if (files.Length == 1)
        //    {
        //        using (var stream = new FileStream(upfile.FileName, FileMode.CreateNew))
        //        {
        //            await upfile.CopyToAsync(stream);
        //        }
        //        return Ok();
        //    }

        //    return NotFound(upfile.FileName);

        //}



        private ModelStateDictionary GetModel(ScriptDiagnostics diagnostics)
        {

            var model = new ModelStateDictionary();

            foreach (var diagnostic in diagnostics)
            {

                string message;

                //if (diagnostic.Location.Start.IsEmpty)
                //{
                //}
                //else
                //    //message += diagnostic.Location.Start.;

                model.AddModelError(diagnostic.Severity + (model.Count + 1).ToString(), diagnostic.ToString());

            }

            return model;

        }

        internal readonly ProjectBuilderProvider _builder;
        private readonly ILogger<MockController> _logger;

    }


}


// dotnet.exe build "/app/tmp/parrot/projects/parcel/mock/service/mock.csproj