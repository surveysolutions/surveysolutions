using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using WB.Services.Export.Events;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.Models;

namespace WB.Services.Export.Jobs
{
    internal class ExportJob : IExportJob
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IEventProcessor processor;
        private readonly IExportProcessHandler<DataExportProcessArgs> exportProcessHandler;
        private readonly ILogger<ExportJob> logger;

        public ExportJob(IServiceProvider serviceProvider,
            IEventProcessor processor,
            ILogger<ExportJob> logger, 
            IExportProcessHandler<DataExportProcessArgs> exportProcessHandler)
        {
            logger.LogTrace("Constructed instance of ExportJob");

            this.serviceProvider = serviceProvider;
            this.processor = processor;
            this.logger = logger;
            this.exportProcessHandler = exportProcessHandler;
        }

        public async Task ExecuteAsync(DataExportProcessArgs pendingExportProcess, CancellationToken cancellationToken)
        {
            var handleCriticalErrors = false;

            try
            {
                serviceProvider.SetTenant(pendingExportProcess.ExportSettings.Tenant);

                await processor.HandleNewEvents(pendingExportProcess.ProcessId, cancellationToken);

                handleCriticalErrors = true;

                await exportProcessHandler.ExportDataAsync(pendingExportProcess, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (PostgresException pe) when (pe.SqlState == "57014") // 57014: canceling statement due to user request
            {
                throw;
            }
            catch (Exception e) when (e.InnerException is TaskCanceledException)
            {
                throw;
            }
            catch (IOException e) when ((e.HResult & 0xFFFF) == 0x70)
            {
                throw;
            }
            catch (Exception e)
            {
                if (handleCriticalErrors)
                    this.logger.LogCritical(e, "Export job failed");
                else
                    this.logger.LogError(e, "Export job failed");

                throw;
            }
        }
    }
}
