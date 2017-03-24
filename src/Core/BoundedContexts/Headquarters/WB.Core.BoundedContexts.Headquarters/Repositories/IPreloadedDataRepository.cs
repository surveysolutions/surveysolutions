using System.IO;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IPreloadedDataRepository
    {
        string StoreSampleData(Stream preloadedDataFile, string fileName);
        string StorePanelData(Stream preloadedDataFile, string fileName);
        PreloadedContentMetaData GetPreloadedDataMetaInformationForSampleData(string id);
        PreloadedContentMetaData GetPreloadedDataMetaInformationForPanelData(string id);
        PreloadedDataByFile GetPreloadedDataOfSample(string id);
        void DeletePreloadedDataOfSample(string id);
        PreloadedDataByFile[] GetPreloadedDataOfPanel(string id);
        void DeletePreloadedDataOfPanel(string id);
        byte[] GetBytesOfSampleData(string id);
    }
}
