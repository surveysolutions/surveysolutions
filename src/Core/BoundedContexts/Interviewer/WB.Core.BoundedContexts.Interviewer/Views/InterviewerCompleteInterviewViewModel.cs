using System.Threading.Tasks;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerCompleteInterviewViewModel : CompleteInterviewViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        readonly IChangeLogManipulator changeLogManipulator;
           
        public InterviewerCompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            ICommandService commandService,
            IPrincipal principal,
            IMvxMessenger messenger, 
            IStatefulInterviewRepository interviewRepository,
            InterviewStateViewModel interviewState, 
            IChangeLogManipulator changeLogManipulator)
            : base(viewModelNavigationService, commandService, principal, messenger, interviewState)
        {
            this.interviewRepository = interviewRepository;
            this.changeLogManipulator = changeLogManipulator;
        }

        public override void Init(string interviewId)
        {
            base.Init(interviewId);

            var statefulInterview = this.interviewRepository.Get(interviewId);
            this.CompleteComment = statefulInterview.InterviewerCompleteComment;
        }

        protected override async Task CloseInterviewAsync()
        {
            this.changeLogManipulator.CloseDraftRecord(interviewId, principal.CurrentUserIdentity.UserId);
            await base.CloseInterviewAsync();
        }
    }
}