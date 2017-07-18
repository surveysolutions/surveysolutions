using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEnumeratorArchiveUtils
    {
        void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath);
        Task ZipDirectoryToFileAsync(string sourceDirectory, string archiveFilePath);

        void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        Task UnzipAsync(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
    }
}
