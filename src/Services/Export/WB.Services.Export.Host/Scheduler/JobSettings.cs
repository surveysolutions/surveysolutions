using System;

namespace WB.Services.Export.Host.Scheduler
{
    public class JobSettings
    {
        public string ConnectionName { get; set; } = "DefaultConnection";
        public int WorkerCount { get; set; } = Environment.ProcessorCount;
        public string SchemaName { get; set; } = "scheduler";
        public int WorkerCountPerTenant { get; set; } = 1;
    }
}
