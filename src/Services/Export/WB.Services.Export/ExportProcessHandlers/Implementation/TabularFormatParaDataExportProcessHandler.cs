//using System;
//using System.Globalization;
//using System.Threading;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using WB.Services.Export.CsvExport;
//using WB.Services.Export.Infrastructure;
//using WB.Services.Export.Interview;
//using WB.Services.Export.Questionnaire.Services;
//using WB.Services.Export.Services.Processing;
//using WB.Services.Export.Utils;

//namespace WB.Services.Export.ExportProcessHandlers.Implementation
//{
//    internal class TabularFormatParaDataExportProcessHandler: AbstractDataExportHandler
//    {
//        private readonly IUserViewFactory userReader;

//        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
//        private readonly ITransactionManagerProvider transactionManagerProvider;
//        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

//        private ITransactionManager TransactionManager => this.transactionManagerProvider.GetTransactionManager();
//        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

//        private void ExecuteInTransaction(Action action) => this.TransactionManager.ExecuteInQueryTransaction(()
//            => this.PlainTransactionManager.ExecuteInPlainTransaction(action.Invoke));

//        private readonly IQuestionnaireStorage questionnaireStorage;
//        private readonly ICsvWriter csvWriter;
//        private readonly ITabularFormatExportService tabularFormatExportService;
//        private readonly ILogger<TabularFormatParaDataExportProcessHandler> logger;

//        public TabularFormatParaDataExportProcessHandler(IEventStore eventStore,
//            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
//            IUserViewFactory userReader,
//            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
//            ITransactionManagerProvider transactionManagerProvider,
//            IDataExportProcessesService dataExportProcessesService, 
//            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage, 
//            IPlainTransactionManagerProvider plainTransactionManagerProvider,
//            IQuestionnaireStorage questionnaireStorage,
//            IFileSystemAccessor fs,
//            IFilebasedExportedDataAccessor dataAccessor,
//            IDataExportFileAccessor exportFileAccessor,
//            ICsvWriter csvWriter,
//            ITabularFormatExportService tabularFormatExportService,
//            ILogger logger) : base(fs, dataAccessor, interviewDataExportSettings, dataExportProcessesService, exportFileAccessor)
//        {
//            this.eventStore = eventStore;
//            this.interviewSummaryReader = interviewSummaryReader;
//            this.userReader = userReader;
//            this.transactionManagerProvider = transactionManagerProvider;
//            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
//            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
//            this.questionnaireStorage = questionnaireStorage;
//            this.csvWriter = csvWriter;
//            this.tabularFormatExportService = tabularFormatExportService;
//            this.logger = logger;
//        }

//        protected override DataExportFormat Format => DataExportFormat.Paradata;

//        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
//            CancellationToken cancellationToken)
//        {
//            var interviewsToExport = this.tabularFormatExportService.GetInterviewsToExport(
//                settings.QuestionnaireId, settings.InterviewStatus, cancellationToken, settings.FromDate,
//                settings.ToDate).ToList();

//            var paradataReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();

//            var interviewParaDataEventHandler = new InterviewParaDataEventHandler(paradataReader,
//                this.interviewSummaryReader, this.userReader, this.interviewDataExportSettings,
//                this.questionnaireExportStructureStorage, this.questionnaireStorage);

//            cancellationToken.ThrowIfCancellationRequested();

//            var exportFilePath = this.fileSystemAccessor.CombinePath(settings.ExportDirectory, "paradata.tab");

//            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(exportFilePath, true))
//            using (var writer = this.csvWriter.OpenCsvWriter(fileStream, ExportFileSettings.DataFileSeparator.ToString()))
//            {
//                writer.WriteField("interview__id");
//                writer.WriteField("#");
//                writer.WriteField("action");
//                writer.WriteField("responsible");
//                writer.WriteField("role");
//                writer.WriteField("timestamp");
//                writer.WriteField("offset");
//                writer.WriteField("parameters");
//                writer.NextRecord();

//                long totalInterviewsProcessed = 0;
//                foreach (var interviewId in interviewsToExport)
//                {
//                    cancellationToken.ThrowIfCancellationRequested();

//                    var eventsByInterview = this.eventStore.Read(interviewId.Id, 0);

//                    try
//                    {
//                        this.ExecuteInTransaction(() => eventsByInterview.ForEach(interviewParaDataEventHandler.Handle));

//                        var paradata = paradataReader.Query(_ => _.FirstOrDefault());
//                        for (int i = 0; i < paradata.Records.Count; i++)
//                        {
//                            var record = paradata.Records[i];
//                            writer.WriteField(interviewId.Id);
//                            writer.WriteField(i + 1);
//                            writer.WriteField(record.Action);
//                            writer.WriteField(record.OriginatorName);
//                            writer.WriteField(record.OriginatorRole);
//                            writer.WriteField(record.Timestamp?.ToString("s", CultureInfo.InvariantCulture) ?? "");
//                            writer.WriteField(record.Offset != null ? record.Offset.Value.ToString() : "");

//                            writer.WriteField(String.Join("||",
//                                record.Parameters.Values.Select(Utils.RemoveNewLine)));
                            
//                            writer.NextRecord();
//                        }

//                        paradataReader.Clear();
//                    }
//                    catch (Exception e)
//                    {
//                        this.logger.Error($"Paradata unhandled exception for interview {interviewId}", e);
//                    }

//                    totalInterviewsProcessed++;
//                    progress.Report(totalInterviewsProcessed.PercentOf(interviewsToExport.Count));
//                }
//            }
//        }
//    }
//}
