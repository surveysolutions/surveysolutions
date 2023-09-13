#nullable enable
using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public class CalendarEventViewModelArgs
{
    public CalendarEventViewModelArgs(Guid? interviewId, string? interviewKey, int assignmentId, Action? okCallback)
    {
        InterviewId = interviewId;
        InterviewKey = interviewKey;
        AssignmentId = assignmentId;
        OkCallback = okCallback;
    }

    public Guid? InterviewId { get; set; }
    public string? InterviewKey { get; set; }
    public int AssignmentId { get; set; }
    public Action? OkCallback { get; set; }
}
