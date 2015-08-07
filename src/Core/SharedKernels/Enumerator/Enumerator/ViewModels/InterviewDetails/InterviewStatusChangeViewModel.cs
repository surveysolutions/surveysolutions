using System;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.BoundedContexts.Tester.ViewModels.InterviewDetails
{
    public class InterviewStatusChangeViewModel : MvxViewModel, IInterviewEntityViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;

        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        public InterviewStatusChangeViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
        }

        Guid interviewId;

        public Identity Identity { get { return null; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = Guid.Parse(interviewId);
        }

        private IMvxCommand startInterviewCommand;
        public IMvxCommand CompleteInterviewCommand
        {
            get
            {
                return this.startInterviewCommand ?? (this.startInterviewCommand = new MvxCommand(async () => await this.StartInterviewAsync()));
            }
        }

        private string completeComment;
        public string CompleteComment
        {
            get { return this.completeComment; }
            set { this.completeComment = value; RaisePropertyChanged(); }
        }

        private async Task StartInterviewAsync()
        {
            await this.commandService.WaitPendingCommandsAsync();

            var completeInterviewCommand = new CompleteInterviewCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                comment: CompleteComment,
                completeTime: DateTime.UtcNow);

            this.commandService.Execute(completeInterviewCommand);

            //this.viewModelNavigationService.NavigateTo<>();
        }
    }
}
