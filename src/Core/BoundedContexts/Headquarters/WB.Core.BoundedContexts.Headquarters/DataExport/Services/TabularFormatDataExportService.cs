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

            var events = eventStore.GetEventsAfterPosition(eventPosition);
            long eventCount = eventStore.CountOfAllEvents();

            int countOfProcessedEvents = 0;
            bool firstSlice = true;
            bool lastSuccessfullyHandledEventWasFoundAtTheFirstSlice = interviewDenormalizerProgress == null;

            foreach (var eventSlice in events)
            {
                if (!eventSlice.Any())
                    continue;

                TransactionManager.ExecuteInQueryTransaction(
                    () =>
                    {
                        foreach (var committedEvent in eventSlice)
                        {
                            if (firstSlice && !lastSuccessfullyHandledEventWasFoundAtTheFirstSlice)
                            {
                                if (committedEvent.EventSourceId ==
                                    interviewDenormalizerProgress.EventSourceIdOfLastSuccessfullyHandledEvent &&
                                    committedEvent.EventSequence ==
                                    interviewDenormalizerProgress.EventSequenceOfLastSuccessfullyHandledEvent)
                                {
                                    lastSuccessfullyHandledEventWasFoundAtTheFirstSlice = true;
                                    eventCount = eventCount - committedEvent.GlobalSequence;
                                }
                                continue;
                            }
                            interviewParaDataEventHandler.Handle(committedEvent);
                            countOfProcessedEvents++;
                        }

                    });

                if (!lastSuccessfullyHandledEventWasFoundAtTheFirstSlice)
                {
                    throw new InvalidOperationException("Start position wasn't found");
                }

                firstSlice = false;
                this.paraDataAccessor.PersistParaDataExport();
                this.UpdateLastHandledEventPosition(eventSlice.Last(), eventSlice.Position);
                var intermediatePercents = (int) ((countOfProcessedEvents/eventCount)*100);
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        dataExportQueue.UpdateDataExportProgress(dataExportProcessId,
                            Math.Min(intermediatePercents, 100)));
            }

            this.paraDataAccessor.ArchiveParaDataExport();

            this.plainTransactionManager.ExecuteInPlainTransaction(
                () => dataExportQueue.UpdateDataExportProgress(dataExportProcessId, 100));
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