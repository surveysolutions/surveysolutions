using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Interview
{
    public interface IInterviewsExporter
    {
        Task ExportInterviewsInTabularFormat( string tenantId,
            string questionnaireIdentity,
            InterviewStatus? status,
            string basePath,
            IProgress<int> progress, 
            CancellationToken cancellationToken, 
            DateTime? fromDate,
            DateTime? toDate);
    }
}
