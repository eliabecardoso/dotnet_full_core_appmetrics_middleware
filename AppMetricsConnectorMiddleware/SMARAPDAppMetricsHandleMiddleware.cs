using AppOwinAppMetrics.Middlewares.Metrics.DI;
using AppOwinAppMetrics.Middlewares.Metrics.DTO;
using AppOwinAppMetrics.Middlewares.Metrics.Helper;
using Microsoft.Owin;
using Newtonsoft.Json;
using SMARAPDAppMetrics.Metrics.Infrastructure.TypeMetrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Unity;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace AppOwinAppMetrics.Middlewares.Metrics
{
    public class SMARAPDAppMetricsHandleMiddleware
    {
        private readonly AppFunc _next;
        private IOwinContext _owinContext;
        private IUnityContainer _container;
        private static IRabbitMQHelper _rabbitMQHelper;
        private string _client;
        private string _application;
        private string _environment;
        private int _interval;
        private int _activeRequests;
        private int _errors4xx;
        private int _errors5xx;

        public SMARAPDAppMetricsHandleMiddleware(AppFunc next, string client, string application, string environment, int interval = 30)
        {
            _next = next;

            _container = UnityConfig.GetMainContainer();
            _rabbitMQHelper = _container.Resolve<IRabbitMQHelper>();

            _client = client;
            _application = application;
            _environment = environment;
            _interval = interval;

            ApplicationMetrics();
        }

        public async Task Invoke(IDictionary<string, object> contextDict)
        {
            TimerMetrics.StartRequest();
            TimerMetrics.StartResponse();
            ++_activeRequests;

            _owinContext = new OwinContext(contextDict);

            await _next.Invoke(contextDict);

            --_activeRequests;
            _errors4xx = _owinContext.Response.StatusCode.ToString()[0] == '4' ? ++_errors4xx : _errors4xx;
            _errors5xx = _owinContext.Response.StatusCode.ToString()[0] == '5' ? ++_errors5xx : _errors5xx;

            RequestMetrics();
        }

        private void RequestMetrics()
        {
            TimerMetrics.StopRequest();
            TimerMetrics.StopResponse();

            var dto = new RequestMetricsDTO
            {
                Client = _client,
                Application = _application,
                Environment = _environment,
                UserCall = _owinContext.Request.Headers.Get("UserId"),
                Endpoint = _owinContext.Request.Path.Value,
                Method = _owinContext.Request.Method,
                StatusCode = _owinContext.Response.StatusCode,
                //
                RequestTime = TimerMetrics.RequestTime,
                ResponseTime = TimerMetrics.ResponseTime,
            };


            SendRabbitMQQueue("RequestMetrics", dto);
        }

        public async Task ApplicationMetrics()
        {
            var process = Process.GetCurrentProcess();

            var lastDate = DateTime.Now;
            var lastProcessorTime = process.TotalProcessorTime.TotalMilliseconds;

            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            while (true)
            {
                process = Process.GetCurrentProcess();
                
                var dto = new ApplicationMetricsDTO
                {
                    Client = _client,
                    Application = _application,
                    Environment = _environment,
                    ActiveRequests = _activeRequests,
                    Errors4xx = _errors4xx,
                    Errors5xx = _errors5xx,
                    CurrentDirectory = Environment.CurrentDirectory,
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCoreCount = Environment.ProcessorCount,
                    TotalPhysicalMemory = memoryValues != null ? memoryValues.TotalVisibleMemorySize : 0d,
                    PhysicalMemoryUsage = process.PrivateMemorySize64,

                    CurrentProcessorTime = process.TotalProcessorTime.TotalMilliseconds,
                    CurrentProcessorDate = DateTime.Now,
                    LastTotalProcessorTime = lastProcessorTime,
                    LastProcessorDate = lastDate,
                };

                lastProcessorTime = dto.CurrentProcessorTime;
                lastDate = dto.CurrentProcessorDate;

                SendRabbitMQQueue("ApplicationMetrics", dto);

                process.Dispose();
                await Task.Delay(TimeSpan.FromSeconds(_interval));
            }
        }

        private static void SendRabbitMQQueue(string queueName, object dto)
        {
            try
            {
                var body = JsonConvert.SerializeObject(dto);

                using (var connection = _rabbitMQHelper.CreateConnection(_rabbitMQHelper.GetConnectionFactory()))
                using (var model = connection.CreateModel())
                {
                    _rabbitMQHelper.CreateQueue(queueName, model);

                    _rabbitMQHelper.WriteMessageOnQueue(body, queueName, model);
                }

                Console.WriteLine($"Message Successfully Written: {queueName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Metrics Error");
                Console.WriteLine(ex.Message);
                Console.WriteLine("--------------");
            }

        }


    }
}