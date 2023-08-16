using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.ExportProcessHandlers
{
    interface IExternalDataClient : IDisposable
    {
        void InitializeDataClient(string accessToken, string refreshToken, TenantInfo tenant);
        /// <summary>
        /// Create if needed application folder on external storage and return it's id
        /// </summary>
        /// <param name="subFolder">Create subfolder for different export types</param>
        /// <returns>Folder Id</returns>
        Task<string> CreateApplicationFolderAsync(string subFolder );
        Task<string> CreateFolderAsync(string folder, string parentFolder);
        Task<long?> GetFreeSpaceAsync(CancellationToken cancellationToken);
        Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default);
    }
}
