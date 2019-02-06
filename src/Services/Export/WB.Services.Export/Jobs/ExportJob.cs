﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Events;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.ExportProcessHandlers.Externals;
using WB.Services.Export.ExportProcessHandlers.Implementation;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Jobs
{
    internal class ExportJob : IExportJob
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ITenantContext tenantContext;

        private readonly ILogger<ExportJob> logger;
        
        public ExportJob(IServiceProvider serviceProvider, ITenantContext tenantContext,
            ILogger<ExportJob> logger)
        {
            logger.LogTrace("Constructed instance of ExportJob");
            this.serviceProvider = serviceProvider;
            this.tenantContext = tenantContext;
            this.logger = logger;
        }

        public async Task ExecuteAsync(DataExportProcessArgs pendingExportProcess, CancellationToken cancellationToken)
        {
            try
            {
                tenantContext.Tenant = pendingExportProcess.ExportSettings.Tenant;

                if (pendingExportProcess.StorageType.HasValue)
                {
                    var handler = this.GetExternalStorageExportHandler(pendingExportProcess.StorageType.Value);
                    await handler.ExportDataAsync(pendingExportProcess, cancellationToken);
                }
                else
                {
                    var handler = this.GetExportHandler(pendingExportProcess.ExportSettings.ExportFormat);
                    await handler.ExportDataAsync(pendingExportProcess, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Export job failed");
                throw;
            }
        }

        private AbstractExternalStorageDataExportHandler GetExternalStorageExportHandler(ExternalStorageType storageType)
        {
            switch (storageType)
            {
                case ExternalStorageType.OneDrive:
                    return serviceProvider.GetService<OnedriveBinaryDataExportHandler>();
                case ExternalStorageType.Dropbox:
                    return serviceProvider.GetService<DropboxBinaryDataExportHandler>();
                case ExternalStorageType.GoogleDrive:
                    return serviceProvider.GetService<GoogleDriveBinaryDataExportHandler>();
                default:
                    throw new NotSupportedException($"Export handler for '{Enum.GetName(typeof(ExternalStorageType), storageType)}' not found");
            }
        }
        private BaseAbstractDataExportHandler GetExportHandler(DataExportFormat format)
        {
            switch (format)
            {
                case DataExportFormat.Binary:
                    return serviceProvider.GetService<BinaryFormatDataExportHandler>();
                case DataExportFormat.Paradata:
                    return serviceProvider.GetService<TabularFormatParaDataExportProcessHandler>();
                case DataExportFormat.Tabular:
                    return serviceProvider.GetService<TabularFormatDataExportHandler>();
                case DataExportFormat.SPSS:
                    return serviceProvider.GetService<SpssFormatExportHandler>();
                case DataExportFormat.STATA:
                    return serviceProvider.GetService<StataFormatExportHandler>();
                default:
                    throw new NotSupportedException($"Export handler for '{format}' not found");
            }
        }
    }
}
