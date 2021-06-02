using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IInterviewPdfService
    {
        void Open(string interviewId, Identity identity);
        void OpenAttachment(string interviewId, Guid attachmentId);
    }
}