using Bb;
using Bb.Curls;
using Bb.MockService;
using System.Reflection;


namespace Mocks.TestProject
{
    [TestClass]
    public class UnitTest1
    {

        public UnitTest1()
        {

             var dir = Assembly.GetExecutingAssembly().Location.AsFile().Directory;
            _contracts = dir.Combine("contracts").AsDirectory();

            //GlobalConfiguration.UseSwagger = true;
            //GlobalConfiguration.TraceAll = true;

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        }

        [TestMethod]
        public void ContractFailed()
        {

            var file = _contracts.GetFiles("contractFailed.yaml").First().FullName;
            string contract = "contract1";
            string host = "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .StartService()
                .Wait(c => c.Status == Bb.Servers.Web.ServiceRunnerStatus.Running)
                )
            {

                CurlInterpreter cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/upload' " +
                    $"-H 'accept: */*' " +
                    $"-H 'Content-Type: multipart/form-data' " +
                    $"-F 'upfile=@{file};type=application/json'";
                var p = cmd.ResultToJson(false);

                Assert.IsFalse(cmd.LastResponse.IsSuccessStatusCode);

            }

        }

        [TestMethod]
        public void Contract1()
        {

            var file = _contracts.GetFiles("contract1.yaml").First().FullName;
            string contract = "contract2";
            string host = "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .StartService()
                .Wait(c => c.Status == Bb.Servers.Web.ServiceRunnerStatus.Running)
                )
            {

                CurlInterpreter cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/upload' " +
                    $"-H 'accept: */*' " +
                    $"-H 'Content-Type: multipart/form-data' " +
                    $"-F 'upfile=@{file};type=application/json'";
                var p = cmd.ResultToString(false);                

                if (cmd.LastResponse.IsSuccessStatusCode)
                {

                    CurlInterpreter cmd2 = $"curl " +
                        $"-X GET 'http://{host}:{port}/proxy/{contract}/api/v40/method1'" +
                        $"-H 'accept: application/json'";

                    p = cmd2.ResultToString();

                }

                cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/clean' " +
                    $"-H 'accept: */*' ";
                p = cmd.ResultToString(false);

            }

        }


        private DirectoryInfo _contracts;

    }

}