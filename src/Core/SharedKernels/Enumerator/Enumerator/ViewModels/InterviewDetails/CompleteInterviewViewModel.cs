using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CompleteInterviewViewModel : MvxNotifyPropertyChanged
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly ICommandService commandService;
        protected readonly IPrincipal principal;

        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal, 
            IMvxMessenger messenger,
            InterviewStateViewModel interviewState)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.messenger = messenger;
            this.InterviewState = interviewState;
        }

        protected Guid interviewId;

        public virtual void Init(string interviewId)
        {
            this.interviewId = Guid.Parse(interviewId);

            InterviewState.Init(interviewId, null);

            var questionsCount = InterviewState.QuestionsCount;
            this.AnsweredCount = InterviewState.AnsweredQuestionsCount;
            this.ErrorsCount = InterviewState.InvalidAnswersCount;
            this.UnansweredCount = questionsCount - this.AnsweredCount;

        }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }


        public InterviewStateViewModel InterviewState { get; set; }

        private IMvxCommand completeInterviewCommand;
        public IMvxCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ?? 
                    (this.completeInterviewCommand = new MvxCommand(async () => await this.CompleteInterviewAsync(), () => !wasThisInterviewCompleted));
            }
        }

        public string CompleteComment { get; set; }

        private bool wasThisInterviewCompleted = false;

        private async Task CompleteInterviewAsync()
        {
            this.wasThisInterviewCompleted = true;
            await this.commandService.WaitPendingCommandsAsync();

            var completeInterviewCommand = new CompleteInterviewCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                comment: this.CompleteComment,
                completeTime: DateTime.UtcNow);

            await this.commandService.ExecuteAsync(completeInterviewCommand);

            await this.CloseInterviewAsync();
        }

        protected virtual async Task CloseInterviewAsync()
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync();

            this.messenger.Publish(new InterviewCompletedMessage(this));
        }
    }
}