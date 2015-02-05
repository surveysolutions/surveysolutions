using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.UI.Capi.FileStorage
{
    public interface IFileStorageService
    {
        void DeleteFile(string filename);

        FileDescription RetrieveFile(string filename);

        FileDescription RetrieveThumb(string filename);

        void StoreFile(FileDescription file);
    }
}