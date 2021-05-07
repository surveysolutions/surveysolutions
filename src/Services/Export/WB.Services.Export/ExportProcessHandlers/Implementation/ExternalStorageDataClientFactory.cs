using System;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.ExportProcessHandlers.Externals;
using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    class ExternalStorageDataClientFactory : IExternalStorageDataClientFactory
    {
        private readonly IServiceProvider services;

        public ExternalStorageDataClientFactory(IServiceProvider services)
        {
            this.services = services;
        }

        public IExternalDataClient? GetDataClient(ExternalStorageType? storageType)
        {
            if (storageType == null)
            {
                return null;
            }

            switch (storageType.Value)
            {
                case ExternalStorageType.Dropbox:
                    return services.GetService<DropboxDataClient>();
                case ExternalStorageType.OneDrive:
                    return services.GetService<OneDriveDataClient>();
                case ExternalStorageType.GoogleDrive:
                    return services.GetService<GoogleDriveDataClient>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(storageType), storageType, null);
            }
        }
    }
}
