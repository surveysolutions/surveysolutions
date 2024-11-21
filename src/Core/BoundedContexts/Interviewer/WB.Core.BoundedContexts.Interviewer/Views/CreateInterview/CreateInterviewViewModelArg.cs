using System;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.CreateInterview
{
    public class CreateInterviewViewModelArg
    {
        public int AssignmentId { get; set; }
        public Guid InterviewId { get; set; }
        public SourceScreen SourceScreen { get; set; }
    }
}
