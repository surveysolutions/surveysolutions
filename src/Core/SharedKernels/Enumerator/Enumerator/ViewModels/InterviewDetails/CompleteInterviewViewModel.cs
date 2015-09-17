using System;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
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
        private readonly IPrincipal principal;
        private readonly IInterviewCompletionService completionService;
        private readonly IStatefulInterviewRepository interviewRepository;


        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal, 
            IInterviewCompletionService completionService, 
            IStatefulInterviewRepository interviewRepository, 
            IMvxMessenger messenger,
            GroupStateViewModel interviewState)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.completionService = completionService;
            this.interviewRepository = interviewRepository;
            this.messenger = messenger;
            this.InterviewState = interviewState;
        }

        Guid interviewId;

        public void Init(string interviewId)
        {
            this.interviewId = Guid.Parse(interviewId);

            InterviewState.Init(interviewId, null, ScreenType.Complete);

            var questionsCount = InterviewState.QuestionsCount;//  interview.CountActiveQuestionsInInterview();
            this.AnsweredCount = InterviewState.AnsweredQuestionsCount;// interview.CountAnsweredQuestionsInInterview();
            this.ErrorsCount = InterviewState.InvalidAnswersCount;// interview.CountInvalidQuestionsInInterview();
            this.UnansweredCount = questionsCount - this.AnsweredCount;
        }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }


        public GroupStateViewModel InterviewState { get; set; }

        private IMvxCommand completeInterviewCommand;
        public IMvxCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ?? 
                    (this.completeInterviewCommand = new MvxCommand(async () => await this.CompleteInterviewAsync(), () => !wasThisInterviewCompleted));
            }
        }

        private string completeComment;
        public string CompleteComment
        {
            get { return this.completeComment; }
            set { this.completeComment = value; this.RaisePropertyChanged(); }
        }

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

            this.completionService.CompleteInterview(this.interviewId, this.principal.CurrentUserIdentity.UserId);

            this.viewModelNavigationService.NavigateToDashboard();

            messenger.Publish(new InterviewCompleteMessage(this));
        }

    }
}