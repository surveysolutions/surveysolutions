using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class RejectInterviewDialogArgs : IActionDialogArgs
{
    public Guid InterviewId { get; }

    public RejectInterviewDialogArgs(Guid interviewId)
    {
        InterviewId = interviewId;
    }
}