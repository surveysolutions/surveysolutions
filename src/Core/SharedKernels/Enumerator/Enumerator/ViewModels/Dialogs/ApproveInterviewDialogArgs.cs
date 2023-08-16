﻿using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class ApproveInterviewDialogArgs : IActionDialogArgs
{
    public Guid InterviewId { get; }

    public ApproveInterviewDialogArgs(Guid interviewId)
    {
        InterviewId = interviewId;
    }
}