using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class DiagnosticsExporter : IDiagnosticsExporter
    {
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;

        private readonly string dataFileExtension = "tab";
        public string DiagnosticsFileName => "interview__diagnostics";

        public DoExportFileHeader[] DiagnosticsFileColumns => new []
        {
            CommonHeaderItems.InterviewKey,
            CommonHeaderItems.InterviewId,
            new DoExportFileHeader("interview__status", "Last status of interview", ExportValueType.NumericInt,
                Enum.GetValues(typeof(InterviewStatus))
                    .Cast<InterviewStatus>().
                    Select(x => new VariableValueLabel(((int)x).ToString(), x.ToString())).ToArray()),
            new DoExportFileHeader("responsible", "Last responsible person", ExportValueType.String),
            new DoExportFileHeader("interviewers", "Number of interviewers who worked on this interview", ExportValueType.NumericInt),
            new DoExportFileHeader("rejections__sup", "How many times this interview was rejected by supervisors", ExportValueType.NumericInt),
            new DoExportFileHeader("rejections__hq", "How many times this interview was rejected by HQ", ExportValueType.NumericInt),
            //new DoExportFileHeader("n_questions_valid", "Number of valid questions"),
            new DoExportFileHeader("entities__errors", "Number of questions and static texts with errors", ExportValueType.NumericInt),
            new DoExportFileHeader("questions__comments", "Number of questions with comments", ExportValueType.NumericInt),
            new DoExportFileHeader("interview__duration", "Active time it took to complete the interview, DD.HH:MM:SS", ExportValueType.String),
            new DoExportFileHeader("n_questions_unanswered", "Number of unanswered questions", ExportValueType.NumericInt),
        };

        public DiagnosticsExporter(
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
           // this.logger = logger;
            this.tenantApi = tenantApi;
        }

        public async Task ExportAsync(List<Guid> interviewIdsToExport, string basePath, 
            TenantInfo tenant, ExportProgress progress, CancellationToken cancellationToken)
        {
            var batchSize = this.interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            string diagnosticsFilePath = Path.Combine(basePath, Path.ChangeExtension(this.DiagnosticsFileName, this.dataFileExtension));
            this.WriteFileHeader(diagnosticsFilePath);

            long totalProcessed = 0;

            var api = this.tenantApi.For(tenant);

            var data = new List<string[]>(batchSize);

            foreach (var interviewsBatch in interviewIdsToExport.Batch(batchSize))
            {
                var interviews = interviewsBatch.ToArray();
                var diagInfos = await api.GetInterviewDiagnosticsInfoBatchAsync(interviews);
                
                foreach (var diagInfo in diagInfos)
                {
                    data.Add(new string[]
                    {
                        diagInfo.InterviewKey,
                        diagInfo.InterviewId.FormatGuid(),
                        ((int)diagInfo.Status).ToString(),
                        diagInfo.ResponsibleName,
                        diagInfo.NumberOfInterviewers.ToString(),
                        diagInfo.NumberRejectionsBySupervisor.ToString(),
                        diagInfo.NumberRejectionsByHq.ToString(),
                        //diagInfoew.NumberValidQuestions.ToString(),
                        diagInfo.NumberInvalidEntities.ToString(),
                        //diagInfoew.NumberUnansweredQuestions.ToString(),
                        diagInfo.NumberCommentedQuestions.ToString(),
                        diagInfo.InterviewDuration != null
                            ? new TimeSpan(diagInfo.InterviewDuration.Value).ToString(@"dd\.hh\:mm\:ss", CultureInfo.InvariantCulture)
                            : string.Empty,
                        diagInfo.NotAnsweredCount != null 
                            ? diagInfo.NotAnsweredCount.Value.ToString(CultureInfo.InvariantCulture) 
                            : string.Empty
                    });
                }

                this.csvWriter.WriteData(diagnosticsFilePath, data, ExportFileSettings.DataFileSeparator.ToString());
                data.Clear();
                totalProcessed+= interviews.Length;
                progress.Report(totalProcessed.PercentOf(interviewIdsToExport.Count));
                cancellationToken.ThrowIfCancellationRequested();
            }

            progress.Report(100);
        }

        public void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(Path.ChangeExtension(this.DiagnosticsFileName, this.dataFileExtension));
            doContent.AppendLine();

            foreach (var exportFileHeader in DiagnosticsFileColumns)
            {
                if (exportFileHeader.AddCaption)
                    doContent.AppendCaptionLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
                else
                {
                    if (exportFileHeader.VariableValueLabels.Any())
                    {
                        doContent.DefineLabel(exportFileHeader.Title, exportFileHeader.VariableValueLabels);
                        doContent.AssignValuesToVariable(exportFileHeader.Title, exportFileHeader.Title);
                    }

                    doContent.AppendLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
                }
            }

            var fileName = $"{DiagnosticsFileName}.{DoFile.ContentFileNameExtension}";
            var contentFilePath = Path.Combine(basePath, fileName);

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString());
        }


        private void WriteFileHeader(string commentsFilePath)
        {
            var diagnosticsHeader = DiagnosticsFileColumns.Select(h => h.Title).ToArray();

            this.csvWriter.WriteData(commentsFilePath, new[] { diagnosticsHeader }, ExportFileSettings.DataFileSeparator.ToString());
        }

    }
}
