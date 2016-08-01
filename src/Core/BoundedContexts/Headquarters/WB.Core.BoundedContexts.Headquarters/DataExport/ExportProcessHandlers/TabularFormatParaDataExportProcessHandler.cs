using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.DenormalizerStorage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class TabularFormatParaDataExportProcessHandler: IExportProcessHandler<ParaDataExportProcessDetails>
    {
        private readonly IStreamableEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IPlainStorageAccessor<UserDocument> userReader;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage;

        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IParaDataAccessor paraDataAccessor;

        private readonly string interviewParaDataEventHandlerName = typeof(InterviewParaDataEventHandler).Name;

        private ITransactionManager TransactionManager => this.transactionManagerProvider.GetTransactionManager();
        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

        private readonly ILogger logger;

        public TabularFormatParaDataExportProcessHandler(IStreamableEventStore eventStore,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IPlainStorageAccessor<UserDocument> userReader,
            InterviewDataExportSettings interviewDataExportSettings,
            ITransactionManagerProvider transactionManagerProvider,
            IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage,
            IDataExportProcessesService dataExportProcessesService, IParaDataAccessor paraDataAccessor, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage, IPlainTransactionManagerProvider plainTransactionManagerProvider,
            ILogger logger)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.transactionManagerProvider = transactionManagerProvider;
            this.lastPublishedEventPositionForHandlerStorage = lastPublishedEventPositionForHandlerStorage;
            this.dataExportProcessesService = dataExportProcessesService;
            this.paraDataAccessor = paraDataAccessor;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.logger = logger;
        }

        public void ExportData(ParaDataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var interviewHistoryReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();

            var interviewParaDataEventHandler =
                new InterviewParaDataEventHandler(interviewHistoryReader, this.interviewSummaryReader, this.userReader,
                    this.interviewDataExportSettings, this.questionnaireExportStructureStorage);

            var interviewDenormalizerProgress =
                this.TransactionManager.ExecuteInQueryTransaction(
                    () =>
                        this.lastPublishedEventPositionForHandlerStorage.GetById(this.interviewParaDataEventHandlerName));

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            EventPosition? eventPosition = null;
            if (interviewDenormalizerProgress != null)
                eventPosition = new EventPosition(interviewDenormalizerProgress.CommitPosition,
                    interviewDenormalizerProgress.PreparePosition,
                    interviewDenormalizerProgress.EventSourceIdOfLastSuccessfullyHandledEvent,
                    interviewDenormalizerProgress.EventSequenceOfLastSuccessfullyHandledEvent);

            this.logger.Info($"Starting paradata creation from the begining = {!eventPosition.HasValue} ");
            Stopwatch watch = Stopwatch.StartNew();
            if (!eventPosition.HasValue)
            {
                this.paraDataAccessor.ClearParaDataFolder();
            }

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var eventSlices = this.eventStore.GetEventsAfterPosition(eventPosition);
            long eventCount = this.eventStore.GetEventsCountAfterPosition(eventPosition);

            int countOfProcessedEvents = 0;
            int persistCount = 0;
            foreach (var eventSlice in eventSlices)
            {
                dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

                IEnumerable<CommittedEvent> events = eventSlice;

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
                        this.paraDataAccessor.StoreInterviewParadata(interviewHistoryView);
                    }

                    interviewHistoryReader.Clear();

                    this.UpdateLastHandledEventPosition(eventSlice.Position);

                    int progressInPercents = ((long) countOfProcessedEvents).PercentOf(eventCount);
                    this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, progressInPercents);
                    persistCount++;
                }
            }

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            this.paraDataAccessor.ArchiveParaDataFolder();

            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, 100);

            this.logger.Info($"Finished paradata creation. Took: {watch.Elapsed:g} ");
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
    }
}