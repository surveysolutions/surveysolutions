using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerActiveStageViewModel : ActiveStageViewModel
    {
        public InterviewerActiveStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            EnumerationStageViewModel enumerationStage)
            : base(interviewViewModelFactory, enumerationStage)
        {
        }

        protected override CompleteInterviewViewModel CompletionInterviewViewModel()
        {
            return this.interviewViewModelFactory.GetNew<InterviewerCompleteInterviewViewModel>();
        }
    }
}