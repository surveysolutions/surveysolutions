using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation
{
    internal class TabularFormatParaDataExportProcessHandler: AbstractDataExportHandler
    {
        private readonly IEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IUserViewFactory userReader;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;

        private void ExecuteInTransaction(Action action) => action.Invoke();

        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ICsvWriter csvWriter;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly ILogger logger;

        public TabularFormatParaDataExportProcessHandler(IEventStore eventStore,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IUserViewFactory userReader,
            InterviewDataExportSettings interviewDataExportSettings,
            IDataExportProcessesService dataExportProcessesService, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage, 
            IQuestionnaireStorage questionnaireStorage,
            IFileSystemAccessor fs,
            IFilebasedExportedDataAccessor dataAccessor,
            IDataExportFileAccessor exportFileAccessor,
            ICsvWriter csvWriter,
            ITabularFormatExportService tabularFormatExportService,
            ILogger logger) : base(fs, dataAccessor, interviewDataExportSettings, dataExportProcessesService, exportFileAccessor)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.csvWriter = csvWriter;
            this.tabularFormatExportService = tabularFormatExportService;
            this.logger = logger;
        }

        protected override DataExportFormat Format => DataExportFormat.Paradata;

        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var interviewsToExport = this.tabularFormatExportService.GetInterviewsToExport(
                settings.QuestionnaireId, settings.InterviewStatus, cancellationToken, settings.FromDate,
                settings.ToDate).ToList();

            var paradataReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();

            var interviewParaDataEventHandler = new InterviewParaDataEventHandler(paradataReader,
                this.interviewSummaryReader, this.userReader, this.interviewDataExportSettings,
                this.questionnaireExportStructureStorage, this.questionnaireStorage);

            cancellationToken.ThrowIfCancellationRequested();

            var exportFilePath = this.fileSystemAccessor.CombinePath(settings.ExportDirectory, "paradata.tab");

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

                long totalInterviewsProcessed = 0;
                foreach (var interviewId in interviewsToExport)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var eventsByInterview = this.eventStore.Read(interviewId.Id, 0);

                    try
                    {
                        this.ExecuteInTransaction(() => eventsByInterview.ForEach(interviewParaDataEventHandler.Handle));

                        var paradata = paradataReader.Query(_ => _.FirstOrDefault());
                        for (int i = 0; i < paradata.Records.Count; i++)
                        {
                            var record = paradata.Records[i];
                            writer.WriteField(interviewId.Id);
                            writer.WriteField(i + 1);
                            writer.WriteField(record.Action);
                            writer.WriteField(record.OriginatorName);
                            writer.WriteField(record.OriginatorRole);
                            writer.WriteField(record.Timestamp?.ToString("s", CultureInfo.InvariantCulture) ?? "");
                            writer.WriteField(record.Offset != null ? record.Offset.Value.ToString() : "");

                            writer.WriteField(String.Join("||",
                                record.Parameters.Values.Select(Utils.RemoveNewLine)));
                            
                            writer.NextRecord();
                        }

                        paradataReader.Clear();
                    }
                    catch (Exception e)
                    {
                        this.logger.Error($"Paradata unhandled exception for interview {interviewId}", e);
                    }

                    totalInterviewsProcessed++;
                    progress.Report(totalInterviewsProcessed.PercentOf(interviewsToExport.Count));
                }
            }
        }
    }
}
