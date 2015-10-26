using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExporter:IDataExporter
    {
        private readonly IDataExportService dataExportService;

        private readonly IQuestionnaireDataExportServiceFactory questionnaireDataExportServiceFactory;

        private readonly ITransactionManagerProvider transactionManagerProvider;

        private readonly IPlainTransactionManager plainTransactionManager;

        ITransactionManager TransactionManager
        {
            get { return transactionManagerProvider.GetTransactionManager(); }
        }
        private readonly ILogger logger;

        public DataExporter(IDataExportService dataExportService, 
            ITransactionManagerProvider transactionManagerProvider, 
            IPlainTransactionManager plainTransactionManager, 
            ILogger logger, 
            IQuestionnaireDataExportServiceFactory questionnaireDataExportServiceFactory)
        {
            this.dataExportService = dataExportService;
            this.transactionManagerProvider = transactionManagerProvider;
            this.plainTransactionManager = plainTransactionManager;
            this.logger = logger;
            this.questionnaireDataExportServiceFactory = questionnaireDataExportServiceFactory;
        }

        public void StartDataExport()
        {
            string dataExportProcessId =
               this.plainTransactionManager.ExecuteInPlainTransaction(
                   () => dataExportService.DeQueueDataExportProcessId());

            if (string.IsNullOrEmpty(dataExportProcessId))
                return;


            var dataExportProcess =
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        dataExportService.GetDataExportProcess(dataExportProcessId));

            if(dataExportProcess==null)
                return;

            var questionnaireDataExportService =
                questionnaireDataExportServiceFactory.CreateQuestionnaireDataExportService(
                    dataExportProcess.DataExportType);

            if (questionnaireDataExportService == null)
                return;
            try
            {
                questionnaireDataExportService.Export(dataExportProcess.QuestionnaireId, dataExportProcess.QuestionnaireVersion, dataExportProcess.DataExportProcessId);
            }
            catch (Exception e)
            {
                logger.Error(
                    string.Format("data export process with id {0} finished with error", dataExportProcessId),
                    e);

                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        dataExportService.FinishDataExportProcessWithError(dataExportProcessId,e));
                return;
            }
            this.plainTransactionManager.ExecuteInPlainTransaction(
                () => dataExportService.FinishDataExportProcess(dataExportProcessId));
        }
    }
}