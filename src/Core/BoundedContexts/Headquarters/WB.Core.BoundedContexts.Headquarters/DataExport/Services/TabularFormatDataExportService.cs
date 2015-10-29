using System;
using System.Text.RegularExpressions;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
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
    internal class TabularFormatDataExportService: IDataExportService
    {
        private readonly IStreamableEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainTransactionManager plainTransactionManager;

        private readonly IDataExportQueue dataExportQueue;
        private readonly IParaDataWriter paraDataWriter;
        ITransactionManager TransactionManager
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
            IParaDataWriter paraDataWriter)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.questionnaireReader = questionnaireReader;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.transactionManagerProvider = transactionManagerProvider;
            this.dataExportQueue = dataExportQueue;
            this.plainTransactionManager = plainTransactionManager;
            this.paraDataWriter = paraDataWriter;
        }

        public void ExportData(Guid questionnaireId, long questionnaireVersion, string dataExportProcessId)
        {
            throw new NotImplementedException();
        }

        public void ExportParaData(string dataExportProcessId)
        {
            var interviewHistoryDenormalizer =
                new InterviewParaDataEventHandler(paraDataWriter, interviewSummaryReader, userReader, questionnaireReader,
                    interviewDataExportSettings);

            paraDataWriter.ClearParaData();

            var eventCount = eventStore.CountOfAllEvents();
            var events = eventStore.GetAllEvents();
            int numberOfEventsToBeHandledInOrderToUpdateExportProgress = eventCount / 100;

            TransactionManager.ExecuteInQueryTransaction(
                () =>
                {
                    int countOfProcessedEvents = 0;
                    foreach (var committedEvent in events)
                    {
                        interviewHistoryDenormalizer.Handle(committedEvent);
                        if (countOfProcessedEvents <= eventCount)
                        {
                            if (countOfProcessedEvents%numberOfEventsToBeHandledInOrderToUpdateExportProgress == 0)
                            {
                                int intermediatePercents = countOfProcessedEvents/
                                                           numberOfEventsToBeHandledInOrderToUpdateExportProgress;
                                this.plainTransactionManager.ExecuteInPlainTransaction(
                                    () =>
                                        dataExportQueue.UpdateDataExportProgress(dataExportProcessId,
                                            intermediatePercents));
                            }
                        }
                        countOfProcessedEvents++;
                    }

                });

            paraDataWriter.CreateParaData();
        }
    }
}