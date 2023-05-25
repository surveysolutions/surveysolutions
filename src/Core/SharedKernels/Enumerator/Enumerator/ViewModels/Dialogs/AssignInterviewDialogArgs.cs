using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class AssignInterviewDialogArgs : IActionDialogArgs
{
    public Guid InterviewId { get; }

    public AssignInterviewDialogArgs(Guid interviewId)
    {
        InterviewId = interviewId;
    }
}