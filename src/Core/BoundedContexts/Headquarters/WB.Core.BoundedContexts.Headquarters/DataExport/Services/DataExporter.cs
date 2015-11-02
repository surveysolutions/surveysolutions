using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExporter : IDataExporter
    {
        private bool IsWorking = false; //please use singleton injection

        private readonly IDataExportQueue dataExportQueue;

        private readonly IQuestionnaireDataExportServiceFactory questionnaireDataExportServiceFactory;

        private readonly IPlainTransactionManager plainTransactionManager;

        private readonly ILogger logger;

        public DataExporter(IDataExportQueue dataExportQueue,
            IPlainTransactionManager plainTransactionManager,
            ILogger logger,
            IQuestionnaireDataExportServiceFactory questionnaireDataExportServiceFactory)
        {
            this.dataExportQueue = dataExportQueue;
            this.plainTransactionManager = plainTransactionManager;
            this.logger = logger;
            this.questionnaireDataExportServiceFactory = questionnaireDataExportServiceFactory;
        }

        public void StartDataExport()
        {
            if (IsWorking)
                return;

            IsWorking = true;
            try
            {
                while (IsWorking)
                {
                    string dataExportProcessId =
                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => this.dataExportQueue.DeQueueDataExportProcessId());

                    if (string.IsNullOrEmpty(dataExportProcessId))
                        return;


                    var dataExportProcess =
                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () =>
                                this.dataExportQueue.GetDataExportProcess(dataExportProcessId));

                    if (dataExportProcess == null)
                        return;

                    var questionnaireDataExportService =
                        questionnaireDataExportServiceFactory.CreateQuestionnaireDataExportService(
                            dataExportProcess.DataExportFormat);

                    if (questionnaireDataExportService == null)
                        return;

                    try
                    {
                        switch (dataExportProcess.DataExportType)
                        {
                            case DataExportType.Data:
                                if (dataExportProcess.QuestionnaireId.HasValue &&
                                    dataExportProcess.QuestionnaireVersion.HasValue)
                                    questionnaireDataExportService.ExportData(dataExportProcess.QuestionnaireId.Value,
                                        dataExportProcess.QuestionnaireVersion.Value,
                                        dataExportProcess.DataExportProcessId);
                                else
                                    throw new ArgumentException(
                                        "QuestionnaireId and QuestionnaireVersion can't be empty for data export");
                                break;
                            case DataExportType.ParaData:
                                questionnaireDataExportService.ExportParaData(dataExportProcess.DataExportProcessId);
                                break;
                        }

                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => this.dataExportQueue.FinishDataExportProcess(dataExportProcessId));
                    }
                    catch (Exception e)
                    {
                        logger.Error(
                            string.Format("data export process with id {0} finished with error", dataExportProcessId),
                            e);

                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () =>
                                this.dataExportQueue.FinishDataExportProcessWithError(dataExportProcessId, e));
                    }
                }
            }
            finally
            {
                IsWorking = false;
            }
        }
    }
}