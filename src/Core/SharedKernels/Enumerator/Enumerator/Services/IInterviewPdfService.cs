using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IInterviewPdfService
    {
        Task OpenAsync(string interviewId, Identity identity);
        Task OpenAttachmentAsync(string interviewId, Guid attachmentId);
    }
}