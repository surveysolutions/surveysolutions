using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WB.Services.Scheduler.Tests")]

namespace WB.Services.Scheduler
{
    public class JobSettings
    {
        public string ConnectionName { get; set; } = "DefaultConnection";
        public int WorkerCount { get; set; } = Environment.ProcessorCount;
        public string SchemaName { get; set; } = "scheduler";
        public int WorkerCountPerTenant { get; set; } = 1;
        public double ClearStaleJobsInSeconds { get; set; } = 60;

        public string WorkerId { get; set; } =  Environment.MachineName + ":" + Assembly.GetExecutingAssembly().GetName().Version;
    }
}
