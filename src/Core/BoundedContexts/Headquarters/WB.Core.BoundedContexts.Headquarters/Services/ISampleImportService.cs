using System;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISampleImportService
    {
        void CreatePanel(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId, Guid? responsibleSupervisorId);
        void CreateSample(Guid questionnaireId, long version, string id, PreloadedDataByFile data, Guid responsibleHeadquarterId, Guid? responsibleSupervisorId);
        SampleCreationStatus GetSampleCreationStatus(string id);
    }
}