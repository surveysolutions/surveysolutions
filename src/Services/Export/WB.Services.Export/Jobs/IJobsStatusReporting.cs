using System;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Jobs
{
    public interface IJobsStatusReporting
    {
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaire(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            string archiveFileName,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        Task<DataExportArchive> DownloadArchive(TenantInfo tenant, string archiveName,
            DataExportFormat dataExportFormat, InterviewStatus? status,
            DateTime? from, DateTime? to);
    }
}
