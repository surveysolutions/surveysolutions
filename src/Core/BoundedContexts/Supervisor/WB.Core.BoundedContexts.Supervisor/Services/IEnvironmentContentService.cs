using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    internal interface IEnvironmentContentService {
        void CreateContentOfAdditionalFile(HeaderStructureForLevel headerStructureForLevel, string dataFileName, string contentFilePath);
        string GetEnvironmentContentFileName(string levelName);
    }
}