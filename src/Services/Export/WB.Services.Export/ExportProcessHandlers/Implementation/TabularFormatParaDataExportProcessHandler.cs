using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Utils;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class TabularFormatParaDataExportProcessHandler : AbstractDataExportHandler
    {
        private readonly ICsvWriter csvWriter;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly ILogger<TabularFormatParaDataExportProcessHandler> logger;

        public TabularFormatParaDataExportProcessHandler(
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            ITenantApi<IHeadquartersApi> tenantApi,
            IDataExportProcessesService dataExportProcessesService,
            IFileSystemAccessor fs,
            IFilebasedExportedDataAccessor dataAccessor,
            IDataExportFileAccessor exportFileAccessor,
            ICsvWriter csvWriter,
            ILogger<TabularFormatParaDataExportProcessHandler> logger) 
            :  base(fs, dataAccessor, interviewDataExportSettings, dataExportProcessesService, exportFileAccessor)
        {
            this.tenantApi = tenantApi;
            this.csvWriter = csvWriter;
            this.logger = logger;
        }

        protected override DataExportFormat Format => DataExportFormat.Paradata;

        protected override async Task ExportDataIntoDirectoryAsync(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Start paradata export for " + settings);
            var api = this.tenantApi.For(settings.Tenant);
            var interviewsToExport = await api.GetInterviewsToExportAsync(settings);
            
            cancellationToken.ThrowIfCancellationRequested();

            var exportFilePath = this.fileSystemAccessor.CombinePath(settings.ExportTempDirectory, "paradata.tab");

            long totalInterviewsProcessed = 0;

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(exportFilePath, true))
            using (var writer = this.csvWriter.OpenCsvWriter(fileStream, ExportFileSettings.DataFileSeparator.ToString()))
            {
                writer.WriteField("interview__id");
                writer.WriteField("#");
                writer.WriteField("action");
                writer.WriteField("responsible");
                writer.WriteField("role");
                writer.WriteField("timestamp");
                writer.WriteField("offset");
                writer.WriteField("parameters");
                writer.NextRecord();

                async Task QueryParadata(IEnumerable<InterviewToExport> interviews)
                {
                    var historyItems = await api.GetInterviewsHistory(interviews.Select(i => i.Id).ToArray());
                    logger.LogTrace($"Query headquarters for interviews history. Got {historyItems.Count} items with {historyItems.Sum(h => h.Records.Count)} records");

                    foreach (InterviewHistoryView paradata in historyItems)
                    {
                        WriteParadata(writer, paradata);
                    }

                    totalInterviewsProcessed += historyItems.Count;
                    progress.Report(totalInterviewsProcessed.PercentOf(interviewsToExport.Count));
                }

                var options = new BatchOptions
                {
                    TargetSeconds = 5,
                    Max = interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery
                };

                foreach (var interviews in interviewsToExport.BatchInTime(options, logger))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await QueryParadata(interviews);
                }
            }

            logger.LogInformation("Completed paradata export for " + settings);
        }

        void WriteParadata(ICsvWriterService writer, InterviewHistoryView paradata)
        {
            lock (writer)
            {
                for (int i = 0; i < paradata.Records.Count; i++)
                {
                    var record = paradata.Records[i];

                    writer.WriteField(paradata.InterviewId);
                    writer.WriteField(i + 1);
                    writer.WriteField(record.Action);
                    writer.WriteField(record.OriginatorName);
                    writer.WriteField(record.OriginatorRole);
                    writer.WriteField(record.Timestamp?.ToString("s", CultureInfo.InvariantCulture) ?? "");
                    writer.WriteField(record.Offset != null ? record.Offset.Value.ToString() : "");
                    writer.WriteField(String.Join("||", record.Parameters.Values.Select(v => v.RemoveNewLine())));

                    writer.NextRecord();
                }
            }
        }
    }
}
