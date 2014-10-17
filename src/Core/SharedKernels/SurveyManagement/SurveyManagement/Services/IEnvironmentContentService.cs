using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IEnvironmentContentService {
        void CreateContentOfAdditionalFile(HeaderStructureForLevel headerStructureForLevel, string dataFileName, string contentFilePath);
        string GetEnvironmentContentFileName(string levelName);
        string ContentFileNameExtension { get; }
    }
}