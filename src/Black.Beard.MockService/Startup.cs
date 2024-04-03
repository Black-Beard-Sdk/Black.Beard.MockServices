using Bb.Services.Chain;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Bb.Servers.Web.Models;
using Bb.Servers.Web;
using Microsoft.Extensions.FileProviders;

namespace Bb.MockService
{


    /// <summary>
    /// Startup class par parameter
    /// </summary>
    public class Startup : ServiceRunnerStartup
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
            : base(configuration)
        {

        }

        public override void ConfigureServices(IServiceCollection services)
        {

            string root = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                root = "c:\\".Combine("tmp", "mocks");

            else
                root = Path.DirectorySeparatorChar + "tmp".Combine("mocks");

            GlobalConfiguration.CurrentDirectoryToWriteGenerators = root.Combine("contracts");
            GlobalConfiguration.DirectoryToTrace = root.Combine("logs");

            GlobalConfiguration.CurrentDirectoryToWriteGenerators.CreateFolderIfNotExists();
            GlobalConfiguration.DirectoryToTrace.CreateFolderIfNotExists();

            GlobalConfiguration.DirectoryToTrace += Path.DirectorySeparatorChar;

            //Trace.TraceInformation("setting directory to generate projects in : " + Configuration.CurrentDirectoryToWriteProjects);
            Trace.TraceInformation("setting directory to output logs : " + GlobalConfiguration.DirectoryToTrace);

            base.ConfigureServices(services);

            services.AddDirectoryBrowser();

        }

        /// <summary>
        /// Configures the custom services.
        /// </summary>
        /// <param name="services"></param>
        public override void AppendServices(IServiceCollection services)
        {
            RegisterServicesPolicies(services);
        }


        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public override void ConfigureApplication(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            base.ConfigureApplication(app, env, loggerFactory);


            //var path = Path.Combine(env.ContentRootPath, "www");
            var requestPath = "/www";
            var fileProvider = new PhysicalFileProvider(GlobalConfiguration.CurrentDirectoryToWriteGenerators);

            app.UseStaticFiles();

            // Enable displaying browser links.
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = requestPath
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = fileProvider,
                RequestPath = requestPath
            });

            app
                .UseHttpsRedirection()
                .UseRouting()
                
                .UseListener()

                //.UseApiKey()                      // Intercept apiKey and create identityPrincipal associated
                //.UseAuthorization()               // Apply authorization for identityPrincipal
                ;

        }


    }



}
