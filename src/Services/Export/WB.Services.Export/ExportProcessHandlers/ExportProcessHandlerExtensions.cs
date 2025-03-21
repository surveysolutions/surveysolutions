using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.ExportProcessHandlers.Externals;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;
using WB.Services.Export.Models;

namespace WB.Services.Export.ExportProcessHandlers
{
    public static class ExportProcessHandlersServiceRegistration
    {
        public static void UseExportProcessHandlers(this IServiceCollection services)
        {
            services.AddTransient<BinaryDataExportHandler>();
            services.AddTransient<BinaryDataExternalStorageDataExportHandler>();
            services.AddTransient<AudioAuditDataExportHandler>();
            services.AddTransient<AudioAuditExternalStorageDataExportHandler>();
            services.AddTransient<TabularFormatParaDataExportProcessHandler>();
            services.AddTransient<TabularFormatDataExportHandler>();
            services.AddTransient<SpssFormatExportHandler>();
            services.AddTransient<StataFormatExportHandler>();
            services.AddTransient<OneDriveDataClient>();
            services.AddTransient<DropboxDataClient>();
            services.AddTransient<GoogleDriveDataClient>();
            services.AddTransient<DdiFormatExportHandler>();

            services.AddTransient<IExternalStorageDataClientFactory, ExternalStorageDataClientFactory>();
            services.AddTransient<IExportHandlerFactory, ExportHandlerFactory>();
            services.AddTransient<IExportProcessHandler<DataExportProcessArgs>, ExportProcessHandler>();
            services.AddTransient<IPublisherToExternalStorage, PublisherToExternalStorage>();
        }
    }
}
