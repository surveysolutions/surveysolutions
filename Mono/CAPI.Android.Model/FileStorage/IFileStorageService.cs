using WB.Core.SharedKernels.DataCollection.Views;

namespace CAPI.Android.Core.Model.FileStorage
{
    public interface IFileStorageService
    {
        void DeleteFile(string filename);

        FileDescription RetrieveFile(string filename);

        FileDescription RetrieveThumb(string filename);

        void StoreFile(FileDescription file);
    }
}