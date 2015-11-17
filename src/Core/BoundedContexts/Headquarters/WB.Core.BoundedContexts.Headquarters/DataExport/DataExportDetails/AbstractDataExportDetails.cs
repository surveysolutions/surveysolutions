using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public abstract class AbstractDataExportDetails : IDataExportDetails
    {
        protected AbstractDataExportDetails(string processName, DataExportFormat format)
        {
            this.ProcessId = Guid.NewGuid().FormatGuid();
            this.BeginDate = DateTime.UtcNow;
            this.LastUpdateDate = DateTime.UtcNow;
            this.Status = DataExportStatus.Queued;
            this.ProgressInPercents = 0;

            this.ProcessName = processName;
            this.Format = format;
        }

        public string ProcessId { get; }
        public string ProcessName { get; }
        public DataExportFormat Format { get; }
        public DateTime BeginDate { get; }

        public DateTime LastUpdateDate { get; set; }
        public DataExportStatus Status { get; set; }
        public int ProgressInPercents { get; set; }
    }
}