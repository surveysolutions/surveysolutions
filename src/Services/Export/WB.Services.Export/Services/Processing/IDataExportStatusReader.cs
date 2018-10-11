using System;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportStatusReader
    {
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaire(TenantInfo tenant, QuestionnaireId questionnaireId,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}