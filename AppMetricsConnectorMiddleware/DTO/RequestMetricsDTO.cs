using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AppOwinAppMetrics.Middlewares.Metrics.DTO
{
    public class RequestMetricsDTO
    {
        public string Client { get; set; }
        public string Environment { get; set; }
        public string Application { get; set; }

        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string EndpointMethod { get { return $"{Endpoint} - {Method}"; } }
        public string UserCall { get; set; }
        public int StatusCode { get; set; }

        public long RequestTime { get; set; }
        public long ResponseTime { get; set; }

        public Int64 UniqueTime
        {
            get {
                Int64 retval = 0;
                var st = new DateTime(1970, 1, 1);
                TimeSpan t = (DateTime.Now.ToUniversalTime() - st);
                retval = (Int64)(t.TotalMilliseconds + 0.5);

                return retval;
            }
        }
    }
}