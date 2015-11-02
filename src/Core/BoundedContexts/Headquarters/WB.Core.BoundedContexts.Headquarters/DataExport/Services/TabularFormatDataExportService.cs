using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class TabularFormatDataExportService : IDataExportService
    {
        private readonly IStreamableEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainTransactionManager plainTransactionManager;
        private readonly IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage;

        private readonly IDataExportQueue dataExportQueue;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly string interviewParaDataEventHandlerName=typeof(InterviewParaDataEventHandler).Name;

        private ITransactionManager TransactionManager
        {
            get { return transactionManagerProvider.GetTransactionManager(); }
        }

        public TabularFormatDataExportService(
            IStreamableEventStore eventStore,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IReadSideRepositoryWriter<UserDocument> userReader,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader,
            InterviewDataExportSettings interviewDataExportSettings,
            ITransactionManagerProvider transactionManagerProvider,
            IDataExportQueue dataExportQueue,
            IPlainTransactionManager plainTransactionManager,
            IParaDataAccessor paraDataAccessor, 
            IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.questionnaireReader = questionnaireReader;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.transactionManagerProvider = transactionManagerProvider;
            this.dataExportQueue = dataExportQueue;
            this.plainTransactionManager = plainTransactionManager;
            this.paraDataAccessor = paraDataAccessor;
            this.lastPublishedEventPositionForHandlerStorage = lastPublishedEventPositionForHandlerStorage;
        }

        public void ExportData(Guid questionnaireId, long questionnaireVersion, string dataExportProcessId)
        {
            throw new NotImplementedException();
        }

        public void ExportParaData(string dataExportProcessId)
        {
            var interviewParaDataEventHandler =
                new InterviewParaDataEventHandler(this.paraDataAccessor, interviewSummaryReader, userReader,
                    questionnaireReader,
                    interviewDataExportSettings);

            var interviewDenormalizerProgress =
                TransactionManager.ExecuteInQueryTransaction(
                    () =>
                        this.lastPublishedEventPositionForHandlerStorage.GetById(this.interviewParaDataEventHandlerName));

            EventPosition? eventPosition = null;
            if (interviewDenormalizerProgress != null)
                eventPosition = new EventPosition(interviewDenormalizerProgress.CommitPosition,
                    interviewDenormalizerProgress.PreparePosition);

            if (!eventPosition.HasValue)
            {
                this.paraDataAccessor.ClearParaData();
            }

            var eventSlices = eventStore.GetEventsAfterPosition(eventPosition);
            long eventCount = eventStore.CountOfAllEvents();

            int countOfProcessedEvents = 0;
            bool firstSlice = true;
            int persistCount = 0;
            CommittedEvent lastHandledEvent = null;
            foreach (var eventSlice in eventSlices)
            {
                if (!eventSlice.Any())
                {
                    if (eventSlice.IsEndOfStream && lastHandledEvent != null)
                    {
                        PersistResults(dataExportProcessId, lastHandledEvent, eventSlice.Position, eventCount,
                            countOfProcessedEvents);
                    }
                    continue;
                }

                IEnumerable<CommittedEvent> events = eventSlice;

                if (firstSlice && interviewDenormalizerProgress != null)
                {
                    events = GetUnpublishedEventsFromTheFirstSlice(eventSlice,
                        interviewDenormalizerProgress.EventSourceIdOfLastSuccessfullyHandledEvent,
                        interviewDenormalizerProgress.EventSequenceOfLastSuccessfullyHandledEvent);

                    if(!events.Any())
                        continue;

                    eventCount = eventCount - events.First().GlobalSequence + 1;
                }

                TransactionManager.ExecuteInQueryTransaction(
                    () =>
                    {
                        foreach (var committedEvent in events)
                        {
                            interviewParaDataEventHandler.Handle(committedEvent);
                            lastHandledEvent = committedEvent;
                            countOfProcessedEvents++;
                        }
                    });

                firstSlice = false;

                if (eventSlice.IsEndOfStream || countOfProcessedEvents > 10000*persistCount)
                {
                    PersistResults(dataExportProcessId, lastHandledEvent, eventSlice.Position, eventCount,
                        countOfProcessedEvents);
                    persistCount++;
                }
            }

            this.paraDataAccessor.ArchiveParaDataExport();

            this.plainTransactionManager.ExecuteInPlainTransaction(
                () => dataExportQueue.UpdateDataExportProgress(dataExportProcessId, 100));
        }

        private void PersistResults(string dataExportProcessId, CommittedEvent lastHandledEvent, EventPosition eventPosition, long totatEventCount, int countOfProcessedEvents)
        {
            this.paraDataAccessor.PersistParaDataExport();
            this.UpdateLastHandledEventPosition(lastHandledEvent, eventPosition);
            this.plainTransactionManager.ExecuteInPlainTransaction(
                () =>
                    dataExportQueue.UpdateDataExportProgress(dataExportProcessId,
                        Math.Min((int)(((double)countOfProcessedEvents / totatEventCount) * 100), 100)));
        }

        private IEnumerable<CommittedEvent> GetUnpublishedEventsFromTheFirstSlice(EventSlice eventSlice,
            Guid eventSourceId, int eventSequence)
        {
            IEnumerable<CommittedEvent> events =
                eventSlice.SkipWhile(x => x.EventSourceId != eventSourceId && x.EventSequence != eventSequence);

            if (!events.Any())
            {
                throw new InvalidOperationException("Start position wasn't found");
            }

            return events.Skip(1);
        }

        private void UpdateLastHandledEventPosition(CommittedEvent lastHandledEvent, EventPosition eventPosition)
        {
            try
            {
                TransactionManager.BeginCommandTransaction();

                this.lastPublishedEventPositionForHandlerStorage.Store(
                    new LastPublishedEventPositionForHandler(this.interviewParaDataEventHandlerName,
                        lastHandledEvent.EventSourceId,
                        lastHandledEvent.EventSequence, eventPosition.CommitPosition, eventPosition.PreparePosition),
                    this.interviewParaDataEventHandlerName);

                TransactionManager.CommitCommandTransaction();
            }
            catch
            {
                TransactionManager.RollbackCommandTransaction();
                throw;
            }
        }
    }
}