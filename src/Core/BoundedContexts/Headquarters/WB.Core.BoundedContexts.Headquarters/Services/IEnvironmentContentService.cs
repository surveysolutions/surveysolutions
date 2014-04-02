using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    internal interface IEnvironmentContentService {
        void CreateContentOfAdditionalFile(HeaderStructureForLevel headerStructureForLevel, string dataFileName, string contentFilePath);
        string GetEnvironmentContentFileName(string levelName);
    }
}