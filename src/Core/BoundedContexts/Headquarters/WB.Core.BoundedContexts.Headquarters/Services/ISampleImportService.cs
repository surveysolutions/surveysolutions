using System;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISampleImportService
    {
        Guid ImportSampleAsync(Guid templateId, ISampleRecordsAccessor recordAccessor);
        ImportResult GetImportStatus(Guid id);
        void CreateSample(Guid id, Guid responsibleHeadquarterId, Guid responsibleSupervisorId);
        SampleCreationStatus GetSampleCreationStatus(Guid id);
    }
}