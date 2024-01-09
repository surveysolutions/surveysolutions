using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public class PlayMediaViewModelArgs
{
    public string InterviewId { get; }
    public Guid AttachmentId { get; }

    public PlayMediaViewModelArgs(string interviewId, Guid attachmentId)
    {
        InterviewId = interviewId;
        AttachmentId = attachmentId;
    }
}
