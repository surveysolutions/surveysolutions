using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
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
        
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
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
            IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor)
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
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        public void ExportData(IQueuedProcess process)
        {
            var paraDataProcess = process as ParaDataQueuedProcess;
            if (paraDataProcess != null)
            {
                ExportParaData(process.DataExportProcessId);
                return;
            }
            var approvedDataProcess = process as ApprovedDataQueuedProcess;
            if (approvedDataProcess != null)
            {
                ExportApprovedData(approvedDataProcess.QuestionnaireId, approvedDataProcess.QuestionnaireVersion,
                    process.DataExportProcessId);
                return;
            }
            var allDataProcess = process as AllDataQueuedProcess;
            if (allDataProcess != null)
            {
                ExportData(allDataProcess.QuestionnaireId, allDataProcess.QuestionnaireVersion,
                    process.DataExportProcessId);
                return;
            }
        }

        private void ExportApprovedData(Guid questionnaireId, long questionnaireVersion, string dataExportProcessId)
        {
            TransactionManager.ExecuteInQueryTransaction(
                () =>
                {
                    filebasedExportedDataAccessor.ReexportApprovedTabularDataFolder(questionnaireId,
                        questionnaireVersion);
                });
        }

        private void ExportData(Guid questionnaireId, long questionnaireVersion, string dataExportProcessId)
        {
            TransactionManager.ExecuteInQueryTransaction(
                () =>
                {
                    filebasedExportedDataAccessor.ReexportTabularDataFolder(questionnaireId,
                        questionnaireVersion);
                });
        }

        private void ExportParaData(string dataExportProcessId)
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
                    interviewDenormalizerProgress.PreparePosition,
                    interviewDenormalizerProgress.EventSourceIdOfLastSuccessfullyHandledEvent,
                    interviewDenormalizerProgress.EventSequenceOfLastSuccessfullyHandledEvent);

            if (!eventPosition.HasValue)
            {
                this.paraDataAccessor.ClearParaData();
            }

            var eventSlices = eventStore.GetEventsAfterPosition(eventPosition);
            long eventCount = eventStore.GetEventsCountAfterPosition(eventPosition);

            int countOfProcessedEvents = 0;
            int persistCount = 0;
            foreach (var eventSlice in eventSlices)
            {
                IEnumerable<CommittedEvent> events = eventSlice;

                TransactionManager.ExecuteInQueryTransaction(
                    () =>
                    {
                        foreach (var committedEvent in events)
                        {
                            interviewParaDataEventHandler.Handle(committedEvent);
                            countOfProcessedEvents++;
                        }
                    });

                if (eventSlice.IsEndOfStream || countOfProcessedEvents > 10000*persistCount)
                {
                    PersistResults(dataExportProcessId, eventSlice.Position, eventCount, countOfProcessedEvents);
                    persistCount++;
                }
            }

            this.paraDataAccessor.ArchiveParaDataExport();

            this.plainTransactionManager.ExecuteInPlainTransaction(
                () => dataExportQueue.UpdateDataExportProgress(dataExportProcessId, 100));
        }

        private void PersistResults(string dataExportProcessId, EventPosition eventPosition, long totatEventCount, int countOfProcessedEvents)
        {
            this.paraDataAccessor.PersistParaDataExport();
            this.UpdateLastHandledEventPosition(eventPosition);
            this.plainTransactionManager.ExecuteInPlainTransaction(
                () =>
                    dataExportQueue.UpdateDataExportProgress(dataExportProcessId,
                        Math.Min((int)(((double)countOfProcessedEvents / totatEventCount) * 100), 100)));
        }

        private void UpdateLastHandledEventPosition(EventPosition eventPosition)
        {
            try
            {
                TransactionManager.BeginCommandTransaction();

                this.lastPublishedEventPositionForHandlerStorage.Store(
                    new LastPublishedEventPositionForHandler(this.interviewParaDataEventHandlerName,
                        eventPosition.EventSourceIdOfLastEvent,
                        eventPosition.SequenceOfLastEvent, eventPosition.CommitPosition, eventPosition.PreparePosition),
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