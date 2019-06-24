using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.ExportProcessHandlers
{
    interface IExternalDataClient
    {
        IDisposable GetClient(string accessToken);
        Task<string> CreateApplicationFolderAsync();
        Task<string> CreateFolderAsync(string applicationFolder, string folderName);
        Task<long?> GetFreeSpaceAsync();
        Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default);
    }
}
