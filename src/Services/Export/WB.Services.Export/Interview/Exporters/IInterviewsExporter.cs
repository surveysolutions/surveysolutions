using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Interview.Exporters
{
    public interface IInterviewsExporter
    {
        Task Export(TenantInfo tenant, QuestionnaireExportStructure questionnaireExportStructure,
            QuestionnaireDocument questionnaire, List<InterviewToExport> interviewsToExport, string basePath,
            Progress<int> progress, CancellationToken cancellationToken);
    }
}
