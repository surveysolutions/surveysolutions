using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public interface IDataExportProcessDetails
    {
        string NaturalId { get; }
        string Name { get; }
        DataExportFormat Format { get; }
        DateTime BeginDate { get; }

        CancellationToken CancellationToken { get; }

        DateTime LastUpdateDate { get; set; }
        DataExportStatus Status { get; set; }
        int ProgressInPercents { get; set; }

        void Cancel();
    }
}