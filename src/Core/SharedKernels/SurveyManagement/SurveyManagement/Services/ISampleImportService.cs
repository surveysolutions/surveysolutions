using System;
using WB.Core.SharedKernels.SurveyManagement.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ISampleImportService
    {
        void CreatePanel(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId, Guid? responsibleSupervisorId);
        void CreateSample(Guid questionnaireId, long version, string id, PreloadedDataByFile data, Guid responsibleHeadquarterId, Guid? responsibleSupervisorId);
        SampleCreationStatus GetSampleCreationStatus(string id);
    }
}