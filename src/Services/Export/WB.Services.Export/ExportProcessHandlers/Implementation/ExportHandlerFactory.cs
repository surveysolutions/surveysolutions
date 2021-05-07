using System;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;
using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    class ExportHandlerFactory : IExportHandlerFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ExportHandlerFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IExportHandler GetHandler(DataExportFormat exportFormat, ExternalStorageType? externalStorageType)
        {
            switch (exportFormat)
            {
                case DataExportFormat.Tabular:
                    return serviceProvider.GetRequiredService<TabularFormatDataExportHandler>();
                case DataExportFormat.STATA:
                    return serviceProvider.GetRequiredService<StataFormatExportHandler>();
                case DataExportFormat.SPSS:
                    return serviceProvider.GetRequiredService<SpssFormatExportHandler>();
                case DataExportFormat.DDI:
                    return serviceProvider.GetRequiredService<DdiFormatExportHandler>();
                case DataExportFormat.Paradata:
                    return serviceProvider.GetRequiredService<TabularFormatParaDataExportProcessHandler>();
                case DataExportFormat.Binary:
                    return externalStorageType == null
                        ? (IExportHandler)serviceProvider.GetRequiredService<BinaryDataIntoArchiveExportHandler>()
                        : (IExportHandler)serviceProvider.GetRequiredService<BinaryDataExternalStorageDataExportHandler>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(exportFormat), exportFormat, null);
            }
        }
    }
}
