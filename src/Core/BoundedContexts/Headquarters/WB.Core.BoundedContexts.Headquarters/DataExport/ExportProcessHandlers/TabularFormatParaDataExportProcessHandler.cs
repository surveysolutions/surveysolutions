using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class TabularFormatParaDataExportProcessHandler: IExportProcessHandler<ParaDataExportProcessDetails>
    {
        private readonly IStreamableEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage;

        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IParaDataAccessor paraDataAccessor;

        private readonly string interviewParaDataEventHandlerName = typeof(InterviewParaDataEventHandler).Name;

        private ITransactionManager TransactionManager
        {
            get { return this.transactionManagerProvider.GetTransactionManager(); }
        }

        public TabularFormatParaDataExportProcessHandler(IStreamableEventStore eventStore,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IReadSideRepositoryWriter<UserDocument> userReader,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader,
            InterviewDataExportSettings interviewDataExportSettings,
            ITransactionManagerProvider transactionManagerProvider,
            IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage,
            IDataExportProcessesService dataExportProcessesService, IParaDataAccessor paraDataAccessor)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.questionnaireReader = questionnaireReader;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.transactionManagerProvider = transactionManagerProvider;
            this.lastPublishedEventPositionForHandlerStorage = lastPublishedEventPositionForHandlerStorage;
            this.dataExportProcessesService = dataExportProcessesService;
            this.paraDataAccessor = paraDataAccessor;
        }

        public void ExportData(ParaDataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var interviewParaDataEventHandler =
                new InterviewParaDataEventHandler(this.paraDataAccessor, this.interviewSummaryReader, this.userReader,
                    this.questionnaireReader,
                    this.interviewDataExportSettings);

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

            if (!eventPosition.HasValue)
            {
                this.paraDataAccessor.ClearParaData();
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
                        foreach (var committedEvent in events)
                        {
                            interviewParaDataEventHandler.Handle(committedEvent);
                            countOfProcessedEvents++;
                        }
                    });

                if (eventSlice.IsEndOfStream || countOfProcessedEvents > 10000 * persistCount)
                {
                    this.PersistResults(dataExportProcessDetails.NaturalId, eventSlice.Position, eventCount, countOfProcessedEvents);
                    persistCount++;
                }
            }

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            this.paraDataAccessor.ArchiveParaDataExport();

            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetails.NaturalId, 100);
        }

        private void PersistResults(string dataExportProcessId, EventPosition eventPosition, long totatEventCount, long countOfProcessedEvents)
        {
            this.paraDataAccessor.PersistParaDataExport();
            this.UpdateLastHandledEventPosition(eventPosition);
            int progressInPercents = countOfProcessedEvents.PercentOf(totatEventCount);
            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessId, progressInPercents);
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