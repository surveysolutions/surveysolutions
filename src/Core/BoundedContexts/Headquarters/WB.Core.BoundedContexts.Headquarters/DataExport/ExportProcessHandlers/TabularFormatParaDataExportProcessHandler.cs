using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Main.DenormalizerStorage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class TabularFormatParaDataExportProcessHandler: AbstractDataExportHandler
    {
        private readonly IStreamableEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IUserViewFactory userReader;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage;

        private readonly string interviewParaDataEventHandlerName = typeof(InterviewParaDataEventHandler).Name;

        private ITransactionManager TransactionManager => this.transactionManagerProvider.GetTransactionManager();
        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

        private readonly ILogger logger;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IExportFileNameService exportFileNameService;
        private readonly ICsvWriter csvWriter;

        public TabularFormatParaDataExportProcessHandler(IStreamableEventStore eventStore,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IUserViewFactory userReader,
            InterviewDataExportSettings interviewDataExportSettings,
            ITransactionManagerProvider transactionManagerProvider,
            IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage,
            IDataExportProcessesService dataExportProcessesService, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage, 
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            ILogger logger,
            IQuestionnaireStorage questionnaireStorage,
            IFileSystemAccessor fs,
            IFilebasedExportedDataAccessor dataAccessor,
            IDataExportFileAccessor exportFileAccessor,
            IExportFileNameService exportFileNameService,
            ICsvWriter csvWriter) : base(fs, dataAccessor, interviewDataExportSettings, dataExportProcessesService, exportFileAccessor)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.transactionManagerProvider = transactionManagerProvider;
            this.lastPublishedEventPositionForHandlerStorage = lastPublishedEventPositionForHandlerStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.logger = logger;
            this.questionnaireStorage = questionnaireStorage;
            this.exportFileNameService = exportFileNameService;
            this.csvWriter = csvWriter;

            this.exportDirectoryName = this.interviewDataExportSettings.ExportedDataFolderName;
        }

        private void UpdateLastHandledEventPosition(EventPosition eventPosition)
        {
            try
            {
                this.TransactionManager.BeginCommandTransaction();

                this.lastPublishedEventPositionForHandlerStorage.Store(
                    new LastPublishedEventPositionForHandler(this.interviewParaDataEventHandlerName,
                        eventPosition.EventSourceIdOfLastEvent,
                        eventPosition.SequenceOfLastEvent, eventPosition.CommitPosition, eventPosition.PreparePosition),
                    this.interviewParaDataEventHandlerName);

                this.TransactionManager.CommitCommandTransaction();
            }
            catch
            {
                this.TransactionManager.RollbackCommandTransaction();
                throw;
            }
        }

        private string exportDirectoryName;
        protected override string ExportDirectoryName => exportDirectoryName;

        protected override DataExportFormat Format => DataExportFormat.Paradata;
        protected override bool CanDeleteTempFolder => !this.GetEventPosition().HasValue;

        protected override void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath,
            IProgress<int> progress, CancellationToken cancellationToken)
        {

            var interviewHistoryReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();

            var interviewParaDataEventHandler = new InterviewParaDataEventHandler(interviewHistoryReader,
                this.interviewSummaryReader, this.userReader, this.interviewDataExportSettings,
                this.questionnaireExportStructureStorage, this.questionnaireStorage);

            var eventPosition = GetEventPosition();

            this.logger.Info($"Starting paradata creation from the begining = {!eventPosition.HasValue} ");
            Stopwatch watch = Stopwatch.StartNew();

            cancellationToken.ThrowIfCancellationRequested();

            var eventSlices = this.eventStore.GetEventsAfterPosition(eventPosition);
            long eventCount = this.eventStore.GetEventsCountAfterPosition(eventPosition);

            cancellationToken.ThrowIfCancellationRequested();

            int countOfProcessedEvents = 0;
            int persistCount = 0;
            this.logger.Info($"Exporting events count of all: {eventCount}");

            foreach (var eventSlice in eventSlices)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IEnumerable<CommittedEvent> events = eventSlice;
                this.logger.Info($"Processing export slice. Sequence of last event: {eventSlice.Position.SequenceOfLastEvent}, " +
                                 $"EventSource: ${eventSlice.Position.EventSourceIdOfLastEvent}");

                this.TransactionManager.ExecuteInQueryTransaction(
                    () =>
                    {
                        this.PlainTransactionManager.ExecuteInPlainTransaction(() =>
                        {
                            foreach (var committedEvent in events)
                            {
                                interviewParaDataEventHandler.Handle(committedEvent);
                                countOfProcessedEvents++;
                            }
                        });
                    });

                if (eventSlice.IsEndOfStream || countOfProcessedEvents > 10000 * persistCount)
                {
                    var allInterviewEvents = interviewHistoryReader.Query(_ => _.ToArray());
                    foreach (var interviewHistoryView in allInterviewEvents)
                    {
                        this.StoreInterviewParadata(interviewHistoryView);
                    }

                    interviewHistoryReader.Clear();

                    this.UpdateLastHandledEventPosition(eventSlice.Position);

                    int progressInPercents = ((long)countOfProcessedEvents).PercentOf(eventCount);
                    progress.Report(progressInPercents);
                    persistCount++;
                }
            }

            this.logger.Info($"Finished paradata creation(without compression). Took: {watch.Elapsed:g} ");

            //because patadata build by all questionnaires, but request by 1 questionnaire only
            this.exportDirectoryName = this.fileSystemAccessor.CombinePath(this.exportDirectoryName, questionnaireIdentity.ToString());
        }

        private EventPosition? GetEventPosition()
        {
            var exportParadataProgress = this.TransactionManager.ExecuteInQueryTransaction(() =>
                this.lastPublishedEventPositionForHandlerStorage.GetById(this.interviewParaDataEventHandlerName));

            return exportParadataProgress != null
                ? (EventPosition?) new EventPosition(exportParadataProgress.CommitPosition,
                    exportParadataProgress.PreparePosition,
                    exportParadataProgress.EventSourceIdOfLastSuccessfullyHandledEvent,
                    exportParadataProgress.EventSequenceOfLastSuccessfullyHandledEvent)
                : null;
        }

        public void StoreInterviewParadata(InterviewHistoryView view)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(view.QuestionnaireId, view.QuestionnaireVersion);
            var questionnairePath = this.exportFileNameService.GetFolderNameForParaDataByQuestionnaire(questionnaireIdentity,
                this.ExportDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(questionnairePath))
                this.fileSystemAccessor.CreateDirectory(questionnairePath);

            var interviewFilePath = this.fileSystemAccessor.CombinePath(questionnairePath, $"{view.InterviewId.FormatGuid()}.tab");

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(interviewFilePath, true))
            using (var writer = this.csvWriter.OpenCsvWriter(fileStream, ExportFileSettings.DataFileSeparator.ToString()))
            {
                foreach (var interviewHistoricalRecordView in view.Records)
                {
                    writer.WriteField(interviewHistoricalRecordView.Action);
                    writer.WriteField(interviewHistoricalRecordView.OriginatorName);
                    writer.WriteField(interviewHistoricalRecordView.OriginatorRole);
                    if (interviewHistoricalRecordView.Timestamp.HasValue)
                    {
                        writer.WriteField(interviewHistoricalRecordView.Timestamp.Value.ToString("d",
                            CultureInfo.InvariantCulture));
                        writer.WriteField(interviewHistoricalRecordView.Timestamp.Value.ToString("T",
                            CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        writer.WriteField(string.Empty);
                        writer.WriteField(string.Empty);
                    }
                    foreach (var value in interviewHistoricalRecordView.Parameters.Values)
                    {
                        writer.WriteField(value ?? string.Empty);
                    }
                    writer.NextRecord();
                }
            }
        }
    }
}