using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Services.Export.Jobs;
using WB.Services.Export.Models;
using WB.Services.Scheduler.Services;
using WB.Services.Scheduler.Services.Implementation;

namespace WB.Services.Export.Host.Jobs
{
    public class ExportJobRunner : IJob
    {
        private readonly IExportJob exportJob;

        public ExportJobRunner(IExportJob exportJob)
        {
            this.exportJob = exportJob;
        }

        public const string Name = "export";

        public Task ExecuteAsync(string arg, JobExecutingContext ctx, CancellationToken token)
        {
            var jobArg = JsonConvert.DeserializeObject<DataExportProcessArgs>(arg);
            jobArg.ProcessId = ctx.Job.Id;
            return this.exportJob.ExecuteAsync(jobArg, token);
        }
    }
}
