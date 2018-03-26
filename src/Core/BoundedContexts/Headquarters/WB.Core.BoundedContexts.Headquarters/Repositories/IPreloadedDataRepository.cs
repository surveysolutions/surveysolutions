using System.IO;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IPreloadedDataRepository
    {
        void Store(Stream preloadedFile);
        PreloadedDataByFile GetPreloadedDataOfSample();
        PreloadedDataByFile[] GetPreloadedDataOfPanel();

        PreloadedFile GetPreloadedDataOfSample1();
        PreloadedFile[] GetPreloadedDataOfPanel1();
        void DeletePreloadedData();
    }
}
