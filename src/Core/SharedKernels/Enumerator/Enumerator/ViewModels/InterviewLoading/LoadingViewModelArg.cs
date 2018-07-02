using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading
{
    public class LoadingViewModelArg
    {
        public Guid InterviewId { get; set; }

        public bool ShouldReopen { get; set; }
    }
}
