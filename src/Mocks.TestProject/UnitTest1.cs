using Bb.Curls;
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
        public void UploadMethod2()
        {

            string host = null; // "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .StartService(out var task))
            {

                CurlInterpreter cmd = $"curl -X 'POST' 'http://localhost:{port}/Mock/Test' -H 'accept: */*' -H 'Content-Type: application/json' -d '\"string\"'";
                var a = cmd.ResultToJson().ToString();

            }

        }

        [TestMethod]
        public void UploadMethod1()
        {

            string host = null; // "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .StartService(out var task))
            {

                CurlInterpreter cmd = $"curl -X 'POST' 'http://localhost:{port}/Mock/parcel/upload' -H 'accept: */*' -H 'Content-Type: multipart/form-data' -F 'upfile=@C:\\Users\\g.beard\\Desktop\\Test curl\\swagger.json;type=application/json'";
                var a = cmd.ResultToJson().ToString();

            }

        }

        [TestMethod]
        public void TestMethod1()
        {

            string host = "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
            .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .StartService(out var task))
            {

                CurlInterpreter cmd = $"curl -X GET \"http://localhost:{port}/proxy/parcel/ParcelTracking/11111\" -H \"accept: application/json\"";
                var a = cmd.ResultToString();

            }

        }

    }

}