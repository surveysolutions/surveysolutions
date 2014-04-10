using System;
using WB.Core.SharedKernels.SurveyManagement.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ISampleImportService
    {
        Guid ImportSampleAsync(Guid templateId, long templateVersion, ISampleRecordsAccessor recordAccessor);
        ImportResult GetImportStatus(Guid id);
        void CreateSample(Guid id, Guid responsibleHeadquarterId, Guid responsibleSupervisorId);
        SampleCreationStatus GetSampleCreationStatus(Guid id);
    }
}