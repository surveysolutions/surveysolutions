using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Interview;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Scheduler
{
    public class TabularExportJob
    {
        private readonly ICsvExport csvExport;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<TabularExportJob> logger;

        public TabularExportJob(ICsvExport csvExport, IHostingEnvironment hostingEnvironment, ILogger<TabularExportJob> logger)
        {
            this.csvExport = csvExport;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
        }

        public void Execute(TenantInfo tenant,
            DataExportProcessDetails details,   
            CancellationToken cancellationToken)
        {
            logger.LogInformation($"Started {GetType().Name} processing");

            csvExport.ExportInterviewsInTabularFormat(tenant,
                details.Questionnaire,
                details.InterviewStatus,
                hostingEnvironment.ContentRootPath,
                details.FromDate,
                details.ToDate).Wait(cancellationToken);

            logger.LogInformation($"Completed {GetType().Name} processing");
        }
    }
}
