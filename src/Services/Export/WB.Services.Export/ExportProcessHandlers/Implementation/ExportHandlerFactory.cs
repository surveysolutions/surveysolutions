using System;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.ExportProcessHandlers.Implementation.Handlers;
using WB.Services.Export.Services.Processing;

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
                    return serviceProvider.GetService<TabularFormatDataExportHandler>();
                case DataExportFormat.STATA:
                    return serviceProvider.GetService<StataFormatExportHandler>();
                case DataExportFormat.SPSS:
                    return serviceProvider.GetService<SpssFormatExportHandler>();
                case DataExportFormat.DDI:
                    return serviceProvider.GetService<DdiFormatExportHandler>();
                case DataExportFormat.Paradata:
                    return serviceProvider.GetService<TabularFormatParaDataExportProcessHandler>();
                case DataExportFormat.Binary:
                    return externalStorageType == null
                        ? (IExportHandler)serviceProvider.GetService<BinaryDataIntoArchiveExportHandler>()
                        : (IExportHandler)serviceProvider.GetService<BinaryDataExternalStorageDataExportHandler>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(exportFormat), exportFormat, null);
            }
        }
    }
}
