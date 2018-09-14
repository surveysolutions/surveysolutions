using System;
using Microsoft.Extensions.Hosting;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Scheduler
{
    public class TabularExportJob
    {
        private readonly ICsvExport csvExport;
        private readonly IHostingEnvironment hostingEnvironment;

        public TabularExportJob(ICsvExport csvExport, IHostingEnvironment hostingEnvironment)
        {
            this.csvExport = csvExport;
            this.hostingEnvironment = hostingEnvironment;
        }

        public void Execute(string tenantBaseUrl,
            TenantId tenantId,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            csvExport.ExportInterviewsInTabularFormat(tenantBaseUrl,
                tenantId,
                questionnaireIdentity,
                status,
                hostingEnvironment.ContentRootPath,
                fromDate,
                toDate).Wait();
        }
    }
}
