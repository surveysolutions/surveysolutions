using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public abstract class AbstractDataExportProcessDetails : IDataExportProcessDetails
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected AbstractDataExportProcessDetails(DataExportFormat format)
        {
            this.BeginDate = DateTime.UtcNow;
            this.LastUpdateDate = DateTime.UtcNow;
            this.Status = DataExportStatus.Queued;
            this.ProgressInPercents = 0;

            this.Format = format;
        }

        public abstract string NaturalId { get; }
        public abstract string Name { get; }

        public DataExportFormat Format { get; }
        public DateTime BeginDate { get; }

        public CancellationToken CancellationToken => this.cancellationTokenSource.Token;

        public DateTime LastUpdateDate { get; set; }
        public DataExportStatus Status { get; set; }
        public int ProgressInPercents { get; set; }

        public void Cancel()
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}
