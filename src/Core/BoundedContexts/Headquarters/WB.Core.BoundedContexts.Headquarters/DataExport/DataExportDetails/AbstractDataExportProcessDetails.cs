using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public abstract class AbstractDataExportProcessDetails : IDataExportProcessDetails
    {
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

        public DateTime LastUpdateDate { get; set; }
        public DataExportStatus Status { get; set; }
        public int ProgressInPercents { get; set; }
    }
}