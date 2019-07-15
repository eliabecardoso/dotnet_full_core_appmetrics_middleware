using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppOwinAppMetrics.Middlewares.Metrics.DTO
{
    public class ApplicationMetricsDTO
    {
        public string Id { get; set; }
        public DateTime Timestamp { get { return DateTime.Now; } }
        public string Client { get; set; }
        public string Environment { get; set; }
        public string Application { get; set; }
        public int ActiveRequests { get; set; }
        public int Errors4xx { get; set; }
        public int Errors5xx { get; set; }

        public string OSVersion { get; set; }
        public string CurrentDirectory { get; set; }
        public int ProcessorCoreCount { get; set; }
        public double TotalPhysicalMemory { get; set; }

        public long PhysicalMemoryUsage { get; set; }

        public double CurrentProcessorTime { get; set; }
        public DateTime CurrentProcessorDate { get; set; }
        public double LastTotalProcessorTime { get; set; }
        public DateTime LastProcessorDate { get; set; }
        
        public Int64 UniqueTime
        {
            get
            {
                Int64 retval = 0;
                var st = new DateTime(1970, 1, 1);
                TimeSpan t = (DateTime.Now.ToUniversalTime() - st);
                retval = (Int64)(t.TotalMilliseconds + 0.5);

                return retval;
            }
        }
    }
}