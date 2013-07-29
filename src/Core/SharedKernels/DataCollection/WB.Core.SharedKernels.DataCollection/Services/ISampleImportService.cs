using System;
using System.Collections.Generic;
using System.IO;
using Main.Core.Documents;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface ISampleImportService
    {
        Guid ImportSampleAsync(Guid templateId, ISampleRecordsAccessor recordAccessor);
        ImportResult GetImportStatus(Guid id);
        void CreateSample(Guid id, Guid responsibleHeadquaterId, Guid responsibleSupervisorId);
    }
}