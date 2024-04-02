using Bb;
using Bb.Curls;
using Bb.Extensions;
using Bb.MockService;
using Bb.OpenApiServices;
using Bb.Services;
using Bb.Services.Chain;
using Microsoft.AspNetCore.Http;
using System.Reflection;


namespace Mocks.TestProject
{
    [TestClass]
    public class UnitTest2
    {        

        public UnitTest2()
        {
            var dir = Assembly.GetExecutingAssembly().Location.AsFile().Directory;
            _contracts = dir.Combine("contracts").AsDirectory();

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }

        [TestMethod]
        public void TestConvetionOfName()
        {

            var file = _contracts.GetFiles("contract3.yaml").First().FullName;
            var _document = file.LoadOpenApiContract();

            var ctx = new ContextGenerator(_contracts.FullName);
            var visitor = new ModelAnalyze(ctx);
            visitor.VisitDocument(_document);

        }

        [TestMethod]
        public void MethodListenOnPost()
        {

            string contract = "contract1";
            string host = "localhost";
            int port = 5000;


            bool executed = false;
            Func<HttpListenerContext, Task> action = async q =>
            {

                var response = q.Context.Response;
                await response.WriteAsync("ok");

                executed = true;
                return;

            };

            using (var service = Program.GetService(new string[] { })

                // Add a new method in the service
                .AddService(contract, "segment1/{arg1}", (listener) => listener.AddPost(c => c.AddParameterPath("arg1")
                                                                               .SetDelegate(action)))
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
                )
            {

                service.Wait(c => c.Status != Bb.Servers.Web.ServiceRunnerStatus.Running);

                CurlInterpreter cmd = $"curl -X 'POST' 'http://{host}:{port}/proxy/{contract}/segment1/value1' ";
                var p = cmd.ResultToString(false);

                Assert.IsTrue(cmd.LastResponse.IsSuccessStatusCode);
                Assert.IsTrue(executed);
                Assert.IsTrue(p == "ok");
            }

        }

        [TestMethod]
        public void MethodListenOnGet()
        {

            string contract = "contract2";
            string host = "localhost";
            int port = 5000;


            bool executed = false;
            Func<HttpListenerContext, Task> action = async q =>
            {

                var response = q.Context.Response;
                await response.WriteAsync("ok");

                executed = true;
                return;

            };

            using (var service = Program.GetService(new string[] { })

                // Add a new method in the service
                .AddService(contract, "segment1/{arg1}", (listener) => listener.AddGet(c => c.AddParameterPath("arg1")
                                                                               .SetDelegate(action)))
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
                )
            {

                service.Wait(c => c.Status != Bb.Servers.Web.ServiceRunnerStatus.Running);

                CurlInterpreter cmd = $"curl -X 'GET' 'http://{host}:{port}/proxy/{contract}/segment1/value1' ";
                var p = cmd.ResultToString(false);

                Assert.IsTrue(cmd.LastResponse.IsSuccessStatusCode);
                Assert.IsTrue(executed);
                Assert.IsTrue(p == "ok");
            }

        }

        [TestMethod]
        public void MethodListenOnPut()
        {

            string contract = "contract3";
            string host = "localhost";
            int port = 5000;


            bool executed = false;
            Func<HttpListenerContext, Task> action = async q =>
            {

                q.TryGetArgument("arg1", out var arg1);

                var response = q.Context.Response;
                await response.WriteAsync(arg1.ToString());

                executed = true;
                return;

            };

            using (var service = Program.GetService(new string[] { })

                // Add a new method in the service
                .AddService(contract, "segment1/{arg1}", (listener) => listener.AddPut(c => c.AddParameterPath("arg1")
                                                                               .SetDelegate(action)))
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
                )
            {

                service.Wait(c => c.Status != Bb.Servers.Web.ServiceRunnerStatus.Running);

                CurlInterpreter cmd = $"curl -X 'PUT' 'http://{host}:{port}/proxy/{contract}/segment1/value1' ";
                var p = cmd.ResultToString(true);

                Assert.IsTrue(cmd.LastResponse.IsSuccessStatusCode);
                Assert.IsTrue(executed);
                Assert.IsTrue(p == "value1");
            }

        }

        [TestMethod]
        public void MethodListenOnDelete()
        {

            string contract = "contract3";
            string host = "localhost";
            int port = 5000;


            bool executed = false;
            Func<HttpListenerContext, Task> action = async q =>
            {

                q.TryGetArgument("arg1", out var arg1);

                var response = q.Context.Response;
                await response.WriteAsync(arg1.ToString());

                executed = true;
                return;

            };

            using (var service = Program.GetService(new string[] { })

                // Add a new method in the service
                .AddService(contract, "segment1/{arg1}", (listener) => listener.AddPut(c => c.AddParameterHeader("arg1", true)
                                                                                             .SetDelegate(action)
                                                                                      )
                           )

                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
                )
            {

                service.Wait(c => c.Status != Bb.Servers.Web.ServiceRunnerStatus.Running);

                CurlInterpreter cmd = $"curl " +
                    $"-X 'PUT' 'http://{host}:{port}/proxy/{contract}/segment1/value2' " +
                    $"-H 'arg1: value1' ";

                var p = cmd.ResultToString(true);

                Assert.IsTrue(cmd.LastResponse.IsSuccessStatusCode);
                Assert.IsTrue(executed);
                Assert.IsTrue(p == "value1");
            }

        }

        [TestMethod]
        public void MethodListenOnSecurityApiKey()
        {

            string contract = "contract3";
            string host = "localhost";
            int port = 5000;
            bool executed = false;

            var r = new ApiKeyReferentiel()
                .Add("1234", "black.beard");

            Func<HttpListenerContext, Task> action = async q =>
            {

                q.TryGetArgument("arg1", out var arg1);

                var response = q.Context.Response;
                await response.WriteAsync(arg1.ToString());

                executed = true;
                return;

            };

            using (var service = Program.GetService(new string[] { })   // Add a new method in the service
                .AddService(contract, "segment1/{arg1}", (listener) => listener.AddPut(c => c.AddParameterHeader("arg1", true)
                                                                                             .SetDelegate(action)
                                                                                             .SetSecurityApiKey(ApiKeyInEnum.Header, "x-api-key", a => a.SetReferential(r))
                                                                                      )
                           )

                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
                )
            {

                service.Wait(c => c.Status != Bb.Servers.Web.ServiceRunnerStatus.Running);

                CurlInterpreter cmd = $"curl " +
                    $"-X 'PUT' 'http://{host}:{port}/proxy/{contract}/segment1/value2' " +
                    $"-H 'arg1: value1' " +
                    $"-H 'x-api-key: 1234' "
                    ;

                var p = cmd.ResultToString(true);

                Assert.IsTrue(cmd.LastResponse.IsSuccessStatusCode);
                Assert.IsTrue(executed);
                Assert.IsTrue(p == "value1");
            }

        }

        [TestMethod]
        public void MethodListenOnPostWithCookie()
        {

            string contract = "contract1";
            string host = "localhost";
            int port = 5000;


            bool executed = false;
            Func<HttpListenerContext, Task> action = async q =>
            {

                if (q.TryGetArgument("arg1", out var arg1))
                {
                    executed = string.Compare(arg1.ToString(), "value1") == 0;
                }

                var response = q.Context.Response;
                await response.WriteAsync("ok");

                return;

            };

            using (var service = Program.GetService(new string[] { })

                // Add a new method in the service
                .AddService(contract, "segment1/{arg1}", (listener) => listener.AddPost(c => c.AddParameterCookie("arg1", true)
                                                                                              .SetDelegate(action)
                                                                                       )
                           )
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
                )
            {

                service.Wait(c => c.Status != Bb.Servers.Web.ServiceRunnerStatus.Running);

                CurlInterpreter cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/proxy/{contract}/segment1/value1' " +
                    $"-b 'arg1: value1' "
                    ;
                var p = cmd.ResultToString(false);

                Assert.IsTrue(cmd.LastResponse.IsSuccessStatusCode);
                Assert.IsTrue(executed);
                Assert.IsTrue(p == "ok");
            }

        }

        [TestMethod]
        public void MethodListenOnPostWithBody()
        {

            string contract = "contract1";
            string host = "localhost";
            int port = 5000;


            bool executed = false;
            Func<HttpListenerContext, Task> action = async q =>
            {

                executed = q.Body != null;

                var response = q.Context.Response;
                await response.WriteAsync("ok");

                return;

            };

            using (var service = Program.GetService(new string[] { })

                // Add a new method in the service
                .AddService(contract, "segment1/{arg1}", (listener) => listener.AddPost(c => c.AddParameterBody()
                                                                                              .SetDelegate(action)
                                                                                       )
                           )
                .AddLocalhostUrlWithDynamicPort("http", host, ref port)
                .Start(true)
                )
            {

                service.Wait(c => c.Status != Bb.Servers.Web.ServiceRunnerStatus.Running);

                CurlInterpreter cmd = $"curl " +
                    $"-X 'POST' 'http://{host}:{port}/proxy/{contract}/segment1/value1' " +
                    "-d '{\"Test\" : 1 }' "
                    ;
                var p = cmd.ResultToString(false);

                Assert.IsTrue(cmd.LastResponse.IsSuccessStatusCode);
                Assert.IsTrue(executed);
                Assert.IsTrue(p == "ok");
            }

        }


        private readonly DirectoryInfo _contracts;

    }

}