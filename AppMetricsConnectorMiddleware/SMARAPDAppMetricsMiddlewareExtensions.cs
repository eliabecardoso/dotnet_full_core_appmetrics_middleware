using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AppOwinAppMetrics.Middlewares.Metrics
{
    public static class SMARAPDAppMetricsMiddlewareExtensions
    {

        public static IAppBuilder UseSMARAPDAppMetricsHandleMiddleware(this IAppBuilder builder)
        {
            string application;
            string client;
            string environment;
            int interval;

            using (StreamReader file = File.OpenText(@"C:\\appsettings.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                var settings = (JObject)JToken.ReadFrom(reader);
                application = settings.GetValue("systemApp")?.ToString();
                client = settings.GetValue("client")?.ToString();
                environment = settings.GetValue("environment")?.ToString();

                interval = (int)settings.GetValue("intervalTimeReportingSeconds");
            }

            return builder.Use<SMARAPDAppMetricsHandleMiddleware>(client, application, environment, interval);
        }
    }
}