using System;
using System.Collections.Generic;
using System.IO;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.DTO;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.SampleDataReaders;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface ISampleImportService
    {
        Guid ImportSampleAsync(Guid templateId, ISampleRecordsAccessor recordAccessor);
        ImportResult GetImportStatus(Guid id);
        void CreateSample(Guid id, Guid responsibleHeadquaterId, Guid responsibleSupervisorId);
        SampleCreationStatus GetSampleCreationStatus(Guid id);
    }
}