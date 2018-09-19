using System;
using System.Collections.Generic;
using System.Threading;
using Hangfire;
using WB.Services.Export.Jobs;
using WB.Services.Export.Services.Processing.Good;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportProcessesService : IDataExportProcessesService
    {
        private readonly IBackgroundJobClient backgroundJobClient;

        public DataExportProcessesService(IBackgroundJobClient backgroundJobClient)
        {
            this.backgroundJobClient = backgroundJobClient;
        }

        public string AddDataExport(DataExportProcessDetails args)
        {
            var job = backgroundJobClient.Enqueue<ExportJob>(j => j.Execute(args, CancellationToken.None));
            return job;
        }

        public IEnumerable<DataExportProcessDetails> GetRunningExportProcesses()
        {
            throw new NotImplementedException();
        }

        public DataExportProcessDetails[] GetAllProcesses()
        {
            throw new NotImplementedException();
        }

        public void FinishExportSuccessfully(string processId)
        {
            throw new NotImplementedException();
        }

        public void FinishExportWithError(string processId, Exception e)
        {
            throw new NotImplementedException();
        }

        public void UpdateDataExportProgress(string processId, int progressInPercents)
        {
            throw new NotImplementedException();
        }

        public void DeleteDataExport(string processId)
        {
            throw new NotImplementedException();
        }

        public void ChangeStatusType(string processId, DataExportStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
