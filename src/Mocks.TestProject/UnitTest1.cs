using Bb.Curls;
using Bb;
using Bb.Servers.Web;
using Bb.MockService;

namespace Mocks.TestProject
{
    [TestClass]
    public class UnitTest1
    {


        public UnitTest1()
        {

            //GlobalConfiguration.UseSwagger = true;
            //GlobalConfiguration.TraceAll = true;

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        }


        [TestMethod]
        public void TestMethod1()
        {
                                    
            int port = 5000;
            using (var service = Bb.MockService.Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", null, ref port)) //.AddLocalhostUrlWithDynamicPort("https", ref port) // Créer un certificat pour utilisation
            {
                
                var t1 = service.RunAsync();
                port = ResolvePort(service);

                CurlInterpreter cmd = $"curl -X GET \"http://localhost:{port}/proxy/parcel/ParcelTracking/11111\" -H \"accept: application/json\"";
                var a = cmd.ResultToJson().ToString();

            }

        }


        private static int ResolvePort(ServiceRunner<Startup> service)
        {
            var uri = service.Addresses.First(c => c.Scheme == "http" && c.Host == "127.0.0.1");
            return uri.Port;
        }


    }
}