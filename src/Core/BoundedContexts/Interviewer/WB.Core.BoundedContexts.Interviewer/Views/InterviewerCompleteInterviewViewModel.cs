using Cirrious.MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerCompleteInterviewViewModel : CompleteInterviewViewModel
    {
        readonly IChangeLogManipulator changeLogManipulator;
           
        public InterviewerCompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService, 
            ICommandService commandService,
            IPrincipal principal,
            IMvxMessenger messenger, 
            InterviewStateViewModel interviewState, 
            IChangeLogManipulator changeLogManipulator)
            : base(viewModelNavigationService, commandService, principal, messenger, interviewState)
        {
            this.changeLogManipulator = changeLogManipulator;
        }

        protected override void CloseInterview()
        {
            this.changeLogManipulator.CloseDraftRecord(interviewId, principal.CurrentUserIdentity.UserId);
            base.CloseInterview();
        }
    }
}