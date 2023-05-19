#nullable enable
using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public class DashboardItemDetailDialogViewModelArgs
{
    public DashboardItemDetailDialogViewModelArgs(Guid? interviewId, int? assignmentId)
    {
        InterviewId = interviewId;
        AssignmentId = assignmentId;
    }

    public Guid? InterviewId { get; set; }
    public int? AssignmentId { get; set; }
}