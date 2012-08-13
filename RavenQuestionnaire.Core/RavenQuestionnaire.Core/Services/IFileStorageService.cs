using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Services
{
    public interface IFileStorageService
    {
        void StoreFile(FileDescription file);
        FileDescription RetrieveFile(string filename);
        FileDescription RetrieveThumb(string filename);
        void DeleteFile(string filename);
    }
}
