using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerCompleteInterviewViewModel : CompleteInterviewViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
           
        public InterviewerCompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            ICommandService commandService,
            IPrincipal principal,
            IMvxMessenger messenger, 
            IStatefulInterviewRepository interviewRepository,
            InterviewStateViewModel interviewState,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            DynamicTextViewModel dynamicTextViewModel)
            : base(viewModelNavigationService, commandService, principal, messenger, entitiesListViewModelFactory, interviewState, dynamicTextViewModel)
        {
            this.interviewRepository = interviewRepository;
        }

        public override void Init(string interviewId, NavigationState navigationState)
        {
            base.Init(interviewId, navigationState);

            var statefulInterview = this.interviewRepository.Get(interviewId);
            this.CompleteComment = statefulInterview.InterviewerCompleteComment;
        }
    }
}