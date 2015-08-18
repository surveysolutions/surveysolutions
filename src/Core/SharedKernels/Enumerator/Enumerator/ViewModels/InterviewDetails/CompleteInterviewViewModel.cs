using System;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CompleteInterviewViewModel : MvxViewModel, IInterviewEntityViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;

        private readonly ICommandService commandService;
        private readonly IUserIdentity userIdentity;
        private readonly IInterviewCompletionService completionService;
        

        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
             IUserIdentity userIdentity, 
            IInterviewCompletionService completionService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.userIdentity = userIdentity;
            this.completionService = completionService;
        }

        Guid interviewId;

        public Identity Identity { get { return null; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = Guid.Parse(interviewId);
        }

        private IMvxCommand completeInterviewCommand;
        public IMvxCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ?? (this.completeInterviewCommand = new MvxCommand(async () => await this.CompleteInterviewAsync()));
            }
        }

        private string completeComment;
        public string CompleteComment
        {
            get { return this.completeComment; }
            set { this.completeComment = value; this.RaisePropertyChanged(); }
        }

        private async Task CompleteInterviewAsync()
        {
            await this.commandService.WaitPendingCommandsAsync();

            var completeInterviewCommand = new CompleteInterviewCommand(
                interviewId: this.interviewId,
                userId: this.userIdentity.UserId,
                comment: this.CompleteComment,
                completeTime: DateTime.UtcNow);

            await this.commandService.ExecuteAsync(completeInterviewCommand);

            this.completionService.CompleteInterview(this.interviewId, this.userIdentity.UserId);

            this.viewModelNavigationService.NavigateToDashboard();
        }
    }
}
