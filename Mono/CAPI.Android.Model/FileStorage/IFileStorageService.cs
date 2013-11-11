using Main.Core.Documents;

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