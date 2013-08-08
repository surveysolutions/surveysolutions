using System;
using WB.Core.BoundedContexts.Supervisor.Views.SampleImport;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface ISampleImportService
    {
        Guid ImportSampleAsync(Guid templateId, ISampleRecordsAccessor recordAccessor);
        ImportResult GetImportStatus(Guid id);
        void CreateSample(Guid id, Guid responsibleHeadquaterId, Guid responsibleSupervisorId);
        SampleCreationStatus GetSampleCreationStatus(Guid id);
    }
}