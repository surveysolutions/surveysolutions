using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    internal interface IEnvironmentContentService {
        string BuildContentOfAdditionalFile(HeaderStructureForLevel headerStructureForLevel, string dataFileName);
        string GetEnvironmentContentFileName(string levelName);
    }
}