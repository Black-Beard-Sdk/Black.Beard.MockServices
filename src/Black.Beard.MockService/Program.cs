using Bb.Servers.Web;
using System.Reflection;

namespace Bb.MockService
{

    internal class Program
    {

        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var service = GetService(args);
            service.Run();
        }

        public static ServiceRunnerBase GetService(string[] args)
        {
              
            var currentAssembly = Assembly.GetEntryAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

            return new ServiceRunner<Startup>(args);

        }

    }

}
