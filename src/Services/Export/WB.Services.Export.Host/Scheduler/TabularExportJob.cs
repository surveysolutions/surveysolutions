using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
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
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            logger.LogInformation("Start processing ");
            csvExport.ExportInterviewsInTabularFormat(tenant,
                questionnaireIdentity,
                status,
                hostingEnvironment.ContentRootPath,
                fromDate,
                toDate).GetAwaiter().GetResult();
        }
    }
}
