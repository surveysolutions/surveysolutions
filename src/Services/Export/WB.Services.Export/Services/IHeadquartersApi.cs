using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.Services
{
    public interface IHeadquartersApi
    {
        Task<List<InterviewComment>> GetInterviewCommentsAsync(Guid interviewId, string tenantId);
        Task<List<InterviewToExport>> GetInterviewsToExportAsync(string questionnaireIdentity,
            InterviewStatus? status,
            DateTime? fromDate, 
            DateTime? toDate,
            string tenantId,
            CancellationToken cancellationToken);
        Task<QuestionnaireDocument> GetQuestionnaireAsync(string questionnaireId, string tenantId);
    }
}
