using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Services
{
    public interface IHeadquartersApi
    {
        Task<List<InterviewComment>> GetInterviewCommentsAsync(string tenantBaseUrl, TenantId tenantId,
            Guid interviewId);
        Task<List<InterviewToExport>> GetInterviewsToExportAsync(string tenantBaseUrl,
            TenantId tenantId,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate);
        Task<QuestionnaireDocument> GetQuestionnaireAsync(string tenantBaseUrl, TenantId tenantId,
            QuestionnaireId questionnaireId);
    }
}
