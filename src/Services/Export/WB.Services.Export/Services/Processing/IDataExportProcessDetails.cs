using System;
using System.Threading;

namespace WB.Services.Export.Services.Processing
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
