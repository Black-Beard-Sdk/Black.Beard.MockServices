using Bb.Servers.Web;
using Bb.Services.Chain;
using System.Reflection;

namespace Bb.MockService
{

    public class Program
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

        public static ServiceMockRunner GetService(string[] args)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));
            var result = new ServiceMockRunner(args);
             return result;
        }

    }


    public class ServiceMockRunner : ServiceRunner<Startup>
    {

        public ServiceMockRunner(string[] args)
            : base(args)
        {
            this._services = new List<(string, string, Action<HttpListener>)>();
        }

        public ServiceMockRunner AddService(string contract, string path, Action<HttpListener> action)
        {

            this._services.Add((contract, path, action));

            return this;

        }

        public override void Run()
        {

            var r = this;

            HttpListenerMiddleware.Configure(_services);
          
            base.Run();

        }

        private readonly List<(string, string, Action<HttpListener>)> _services;

    }

    public static class ServiceRunnerBaseExtensions
    {

        /// <summary>
        /// Runs asynchronous service
        /// </summary>
        /// <param name="waitRunning">if set to <c>true</c> [wait service running].</param>
        /// <returns></returns
        public static T Start<T>(this T self, bool waitRunning = true)
            where T : ServiceRunnerBase
        {

            self.RunAsync();

            if (waitRunning)
                while (self.Status != ServiceRunnerStatus.Running)
                {
                    Thread.Sleep(0);
                }

            return self;

        }

        /// <summary>
        /// wait the predicate is true before continue
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T Wait<T>(this T self, Func<ServiceRunnerBase, bool> predicate)
            where T : ServiceRunnerBase
        {

            while (predicate(self))
            {
                Thread.Sleep(0);
            }

            return self;

        }

        /// <summary>
        /// Runs asynchronous service
        /// </summary>
        /// <param name="waitRunning">if set to <c>true</c> [wait service running].</param>
        /// <returns></returns
        public static T RunAsync<T>(this T self)
            where T : ServiceRunnerBase
        {

            if (self.Status != ServiceRunnerStatus.Stopped)
                throw new InvalidOperationException("Service is already running");

            Task.Run(() => self.Run(), self.CancellationToken);

            return self;

        }

    }


}
