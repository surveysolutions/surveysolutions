using System.IO;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IPreloadedDataRepository
    {
        string Store(Stream preloadedFile, string fileName);
        PreloadedContentMetaData GetPreloadedDataMetaInformationForSampleData(string id);
        PreloadedContentMetaData GetPreloadedDataMetaInformationForPanelData(string id);
        PreloadedDataByFile GetPreloadedDataOfSample(string id);
        PreloadedDataByFile[] GetPreloadedDataOfPanel(string id);
        void DeletePreloadedData(string id);
    }
}
