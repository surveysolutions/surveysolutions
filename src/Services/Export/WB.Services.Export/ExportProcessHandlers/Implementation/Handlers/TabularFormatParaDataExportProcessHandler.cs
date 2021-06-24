using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    public class TabularFormatParaDataExportProcessHandler : IExportHandler
    {
        private readonly ICsvWriter csvWriter;
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewsToExportSource interviewsToExportSource;
        private readonly ILogger<TabularFormatParaDataExportProcessHandler> logger;

        public TabularFormatParaDataExportProcessHandler(
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            ITenantApi<IHeadquartersApi> tenantApi,
            IFileSystemAccessor fileSystemAccessor,
            IInterviewsToExportSource interviewsToExportSource,
            ICsvWriter csvWriter,
            ILogger<TabularFormatParaDataExportProcessHandler> logger)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.tenantApi = tenantApi;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewsToExportSource = interviewsToExportSource;
            this.csvWriter = csvWriter;
            this.logger = logger;
        }

        public DataExportFormat Format => DataExportFormat.Paradata;

        public Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        {
            var settings = state.Settings;

            logger.LogInformation("Start paradata export for {settings}", settings);
            var api = this.tenantApi.For(settings.Tenant);
            if(api == null) throw new InvalidOperationException("Api must be not null.");

            var interviewsToExport = this.interviewsToExportSource.GetInterviewsToExport(settings.QuestionnaireId,
                settings.Status, settings.FromDate, settings.ToDate);
            
            cancellationToken.ThrowIfCancellationRequested();

            var exportFilePath = this.fileSystemAccessor.CombinePath(state.ExportTempFolder, "paradata.tab");

            long totalInterviewsProcessed = 0;

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(exportFilePath, true))
            using (var writer = this.csvWriter.OpenCsvWriter(fileStream, ExportFileSettings.DataFileSeparator.ToString()))
            {
                writer.WriteField("interview__id");
                writer.WriteField("order");
                writer.WriteField("event");
                writer.WriteField("responsible");
                writer.WriteField("role");
                writer.WriteField("timestamp_utc");
                writer.WriteField("tz_offset");
                writer.WriteField("parameters");
                writer.NextRecord();

                async Task QueryParadata(IEnumerable<InterviewToExport> interviews)
                {
                    if (api == null) throw new InvalidOperationException("Api must be not null.");
                    var historyItems = await api.GetInterviewsHistory(interviews.Select(i => i.Id).ToArray());
                    logger.LogTrace("Query headquarters for interviews history. Got {historyItemsCount} items with {historyItemsSum} records",
                        historyItems.Count, historyItems.Sum(h => h.Records.Count));

                    if (writer == null) throw new InvalidOperationException("Writer must be not null.");

                    foreach (InterviewHistoryView paradata in historyItems)
                    {
                        WriteParadata(writer, paradata);
                    }

                    totalInterviewsProcessed += historyItems.Count;
                    state.Progress.Report(totalInterviewsProcessed.PercentOf(interviewsToExport!.Count));
                }

                var options = new BatchOptions
                {
                    TargetSeconds = 2,
                    Max = interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery
                };

                var split = interviewsToExport.Batch(Math.Max(1, interviewsToExport.Count / 2));

                var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = 2};

                Parallel.ForEach(split, parallelOptions, exports =>
                {
                    foreach (var interviews in exports.ToList().BatchInTime(options, logger))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        QueryParadata(interviews).Wait(cancellationToken);
                    }
                });
            }

            WriteDoFile(state.ExportTempFolder);

            logger.LogInformation("Completed paradata export for {settings}", settings);

            return Task.CompletedTask;
        }

        private void WriteDoFile(string tempFolder)
        {
            var doFilePath = this.fileSystemAccessor.CombinePath(tempFolder, "paradata.do");
            var doFileContent = new StringBuilder();

            doFileContent.AppendLine("insheet using \"paradata.tab\", tab case names");

            doFileContent.AppendLine("label variable interview__id `\"Unique 32-character long identifier of the interview\"'");
            doFileContent.AppendLine("label variable order `\"Sequential event number within each interview\"'");

            //values are exported as strings yet
            //doFileContent.AppendLine("label define event " + string.Join("",
            //                             Enum.GetValues(typeof(InterviewHistoricalAction)).Cast<InterviewHistoricalAction>()
            //                                 .Select(x => $"{x} `\"{x.ToString()}\"'")));
            
            //doFileContent.AppendLine("label values event event");
            doFileContent.AppendLine("label variable event `\"Type of event happened\"'");
            doFileContent.AppendLine("label variable responsible `\"Login name of the person who initiated the event\"'");
            
            doFileContent.AppendLine("label define role " 
                                     + string.Join("", ExportHelper.RolesMap.Select(x=> $"{x.Key} `\"{x.Value}\"' ")));
            doFileContent.AppendLine("label values role role");
            doFileContent.AppendLine("label variable role `\"System role of the person who initiated the event\"'");
            
            doFileContent.AppendLine("label variable timestamp_utc `\"Date and time when the event happened\"'");
            doFileContent.AppendLine("label variable tz_offset `\"Timezone offset relative to UTC\"'");
            doFileContent.AppendLine("label variable parameters `\"Event-specific parameters\"'");
            doFileContent.AppendLine("");

            this.fileSystemAccessor.WriteAllText(doFilePath, doFileContent.ToString());
        }

        void WriteParadata(ICsvWriterService writer, InterviewHistoryView paradata)
        {
            lock (writer)
            {
                for (int i = 0; i < paradata.Records.Count; i++)
                {
                    var record = paradata.Records[i];

                    writer.WriteField(paradata.InterviewId.FormatGuid());
                    writer.WriteField(i + 1);
                    writer.WriteField(record.Action);
                    writer.WriteField(record.OriginatorName);
                    writer.WriteField(ExportHelper.GetParadataRole(record.OriginatorRole));
                    writer.WriteField(record.Timestamp?.ToString("s", CultureInfo.InvariantCulture) ?? "");
                    writer.WriteField(record.Offset != null ? record.Offset.Value.ToString() : "");
                    writer.WriteField(String.Join("||", record.Parameters.Values.Select(v => v.RemoveNewLine())));

                    writer.NextRecord();
                }
            }
        }
    }
}
