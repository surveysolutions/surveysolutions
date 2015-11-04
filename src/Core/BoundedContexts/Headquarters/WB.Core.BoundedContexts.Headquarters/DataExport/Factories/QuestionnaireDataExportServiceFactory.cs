using System;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal class QuestionnaireDataExportServiceFactory: IQuestionnaireDataExportServiceFactory
    {
        private readonly IStreamableEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IReadSideRepositoryWriter<UserDocument> userReader;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainTransactionManager plainTransactionManager;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage;

        private readonly IDataExportQueue dataExportQueue;
        public QuestionnaireDataExportServiceFactory(
            IStreamableEventStore eventStore, 
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader, 
            IReadSideRepositoryWriter<UserDocument> userReader, 
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireReader, 
            InterviewDataExportSettings interviewDataExportSettings, 
            ITransactionManagerProvider transactionManagerProvider, 
            IDataExportQueue dataExportQueue,
            IPlainTransactionManager plainTransactionManager, 
            IParaDataAccessor paraDataAccessor, 
            IReadSideRepositoryWriter<LastPublishedEventPositionForHandler> lastPublishedEventPositionForHandlerStorage, ITabularFormatExportService tabularFormatExportService, IFilebasedExportedDataAccessor filebasedExportedDataAccessor)
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
            this.tabularFormatExportService = tabularFormatExportService;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        public IDataExportService CreateQuestionnaireDataExportService(DataExportFormat dataExportFormat)
        {
            if (dataExportFormat == DataExportFormat.TabularData)
                return new TabularFormatDataExportService(eventStore, interviewSummaryReader, userReader,
                    questionnaireReader, interviewDataExportSettings, transactionManagerProvider, dataExportQueue,
                    plainTransactionManager, this.paraDataAccessor, lastPublishedEventPositionForHandlerStorage, filebasedExportedDataAccessor, tabularFormatExportService);

            throw new NotSupportedException();
        }
    }
}