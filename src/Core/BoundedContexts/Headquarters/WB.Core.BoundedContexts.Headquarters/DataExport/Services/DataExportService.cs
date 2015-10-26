using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class DataExportService: IDataExportService
    {
        private readonly IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage;

        public DataExportService(IPlainStorageAccessor<DataExportProcessDto> dataExportProcessDtoStorage)
        {
            this.dataExportProcessDtoStorage = dataExportProcessDtoStorage;
        }

        public string DeQueueDataExportProcessId()
        {
            var exportProcess = dataExportProcessDtoStorage.Query(_ => _.Where(p => p.Status == DataExportStatus.Queued)
                .OrderBy(p => p.LastUpdateDate)
                .FirstOrDefault());

            if (exportProcess == null)
                return null;

            exportProcess.Status = DataExportStatus.Running;
            exportProcess.LastUpdateDate = DateTime.UtcNow;

            dataExportProcessDtoStorage.Store(exportProcess, exportProcess.DataExportProcessId);

            return exportProcess.DataExportProcessId;
        }

        public string EnQueueDataExportProcess(Guid questionnaireId, long questionnaireVersion,
            DataExportType exportType)
        {
            var hasRunningOrQueuedDataExportProcessesByTheQuestionnaire =
                dataExportProcessDtoStorage.Query(
                    _ =>
                        _.Any(
                            p =>
                                p.QuestionnaireId == questionnaireId && p.QuestionnaireVersion == questionnaireVersion &&
                                (p.Status == DataExportStatus.Queued || p.Status == DataExportStatus.Running)));

            if (hasRunningOrQueuedDataExportProcessesByTheQuestionnaire)
                throw new InvalidOperationException();

            string processId = Guid.NewGuid().FormatGuid();

            dataExportProcessDtoStorage.Store(
                new DataExportProcessDto()
                {
                    BeginDate = DateTime.UtcNow,
                    DataExportProcessId = processId,
                    DataExportType = exportType,
                    LastUpdateDate = DateTime.UtcNow,
                    ProgressInPercents = 0,
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Status = DataExportStatus.Queued
                }, processId);

            return processId;
        }

        public DataExportProcessDto GetDataExportProcess(string processId)
        {
            return dataExportProcessDtoStorage.GetById(processId);
        }

        public void FinishDataExportProcess(string processId)
        {
            var dataExportProcess = GetDataExportProcess(processId);
            if(dataExportProcess== null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status=DataExportStatus.Finished;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = 100;

            dataExportProcessDtoStorage.Store(dataExportProcess, dataExportProcess.DataExportProcessId);
        }

        public void FinishDataExportProcessWithError(string processId, Exception e)
        {
            var dataExportProcess = GetDataExportProcess(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.Status = DataExportStatus.FinishedWithError;
            dataExportProcess.LastUpdateDate = DateTime.UtcNow;

            dataExportProcessDtoStorage.Store(dataExportProcess, dataExportProcess.DataExportProcessId);
        }

        public void UpdateDataExportProgress(string processId, int progressInPercents)
        {
            if (progressInPercents < 0 || progressInPercents > 100)
                throw new ArgumentException();

            var dataExportProcess = GetDataExportProcess(processId);
            if (dataExportProcess == null || dataExportProcess.Status != DataExportStatus.Running)
                throw new InvalidOperationException();

            dataExportProcess.LastUpdateDate = DateTime.UtcNow;
            dataExportProcess.ProgressInPercents = progressInPercents;

            dataExportProcessDtoStorage.Store(dataExportProcess, dataExportProcess.DataExportProcessId);
        }
    }
}