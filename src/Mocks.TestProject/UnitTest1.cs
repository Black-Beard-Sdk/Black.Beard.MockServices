using Bb;
using Bb.Curls;
using Bb.MockService;
using Bb.Servers.Web;
using Newtonsoft.Json.Linq;
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
                .Start(true)
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
                .Start(true)
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
                        $"-X POST 'http://{host}:{port}/proxy/{contract}/api/v41/method1' " +
                        $"-H 'accept: application/json' " +
                        "-d '[ ]' ";

                    p = cmd2.ResultToString();

#if DEBUG
                    if (!cmd2.LastResponse.IsSuccessStatusCode)
                    {
                        // LocalDebug.Stop();
                    }
#endif

                }

                cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/clean' " +
                    $"-H 'accept: */*' ";
                p = cmd.ResultToString(false);

            }

        }

        [TestMethod]
        public void Contract2CallFailed()
        {

            int code = 0;
            var file = _contracts.GetFiles("contract2.yaml").First().FullName;
            string contract = "contract2";
            string host = "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
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
                        $"-X POST 'http://{host}:{port}/proxy/{contract}/v42/cursor2' " +
                        $"-H 'accept: application/json' " +
                        "-d '[ { \"Data1\": [ \"2008-02-02\", \"2010-05-03\"] } ]' ";

                    p = cmd2.ResultToString();

                    code = cmd2.LastResponse.StatusCode;
                }


                cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/clean' " +
                    $"-H 'accept: */*' ";
                var p2 = cmd.ResultToString(false);

                Assert.AreEqual(code, 400);

            }

        }

        [TestMethod]
        public void Contract2CallSuccessful()
        {

            int code = 0;
            var file = _contracts.GetFiles("contract2.yaml").First().FullName;
            string contract = "contract2";
            string host = "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
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
                        $"-X POST 'http://{host}:{port}/proxy/{contract}/v42/cursor2' " +
                        $"-H 'accept: application/json' " +
                        "-d '[ { \"results\": [ { \"Data1\": [ \"2008-02-02\", \"2010-05-03\"] } ] } ]' ";

                    p = cmd2.ResultToString();

                    code = cmd2.LastResponse.StatusCode;
                }


                cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/clean' " +
                    $"-H 'accept: */*' ";
                var p2 = cmd.ResultToString(false);

                Assert.AreEqual(code, 200);

            }

        }

        [TestMethod]
        public void Contract3()
        {

            int code = 0;
            var file = _contracts.GetFiles("contract3.yaml").First().FullName;
            string contract = "contract3";
            string host = "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
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
                        $"-X POST 'http://{host}:{port}/proxy/{contract}/v43' " +
                        $"-H 'accept: application/json' " +
                        "-d '[ { \"key\": \"key1\" }, { \"key\": \"key2\" } ]' ";

                    p = cmd2.ResultToString();

                    code = cmd2.LastResponse.StatusCode;
                }


                cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/clean' " +
                    $"-H 'accept: */*' ";
                var p2 = cmd.ResultToString(false);

                Assert.AreEqual(code, 200);

            }

        }

        [TestMethod]
        public void Contract4()
        {

            int code = 0;
            var file = _contracts.GetFiles("contract4.yaml").First().FullName;
            string contract = "contract4";
            string host = "localhost";
            int port = 5000;
            string key = "key6";

            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
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
                        $"-X GET 'http://{host}:{port}/proxy/{contract}/v43/{key}'";

                    p = cmd2.ResultToString();

                    code = cmd2.LastResponse.StatusCode;

                    JToken token = JToken.Parse(p);
                    Assert.AreEqual(token["key"], key);

                }


                cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/clean' " +
                    $"-H 'accept: */*' ";
                var p2 = cmd.ResultToString(false);

                Assert.AreEqual(code, 200);

            }

        }

        [TestMethod]
        public void Contract5()
        {

            int code = 0;
            var file = _contracts.GetFiles("contract5.yaml").First().FullName;
            string contract = "contract5";
            string host = "localhost";
            int port = 5000;
            using (var service = Program.GetService(new string[] { })
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
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
                        $"-X POST 'http://{host}:{port}/proxy/{contract}/v43' " +
                        $"-H 'accept: application/json' " +
                        "-d '{ \"key\": \"key1\" }' ";

                    p = cmd2.ResultToString();

                    code = cmd2.LastResponse.StatusCode;
                }


                cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/Mock/{contract}/clean' " +
                    $"-H 'accept: */*' ";
                var p2 = cmd.ResultToString(false);

                Assert.AreEqual(code, 200);

            }

        }



        private DirectoryInfo _contracts;

    }

}