using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CompleteInterviewViewModel : MvxViewModel, IDisposable
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly ICommandService commandService;
        private readonly IEntitiesListViewModelFactory entitiesListViewModelFactory;
        private readonly ILastCompletionComments lastCompletionComments;
        protected readonly IPrincipal principal;

        public InterviewStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }
        public string CompleteScreenTitle { get; set; }

        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal, 
            IMvxMessenger messenger,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            ILastCompletionComments lastCompletionComments,
            InterviewStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel,
            ILogger logger)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.messenger = messenger;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.lastCompletionComments = lastCompletionComments;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.logger = logger;
        }

        private readonly ILogger logger;

        protected Guid interviewId;

        public virtual void Configure(string interviewId,
            NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            this.interviewId = Guid.Parse(interviewId);

            this.InterviewState.Init(interviewId, null);
            this.Name.InitAsStatic(UIResources.Interview_Complete_Screen_Title);

            this.CompleteScreenTitle = UIResources.Interview_Complete_Screen_Description;

            var questionsCount = InterviewState.QuestionsCount;
            this.AnsweredCount = InterviewState.AnsweredQuestionsCount;
            this.ErrorsCount = InterviewState.InvalidAnswersCount;
            this.UnansweredCount = questionsCount - this.AnsweredCount;

            this.EntitiesWithErrors =
                    this.entitiesListViewModelFactory.GetEntitiesWithErrors(interviewId, navigationState).ToList();

            this.EntitiesWithErrorsDescription = EntitiesWithErrors.Count < this.ErrorsCount
                ? string.Format(UIResources.Interview_Complete_First_n_Entities_With_Errors,
                    this.entitiesListViewModelFactory.MaxNumberOfEntities)
                : UIResources.Interview_Complete_Entities_With_Errors;

            this.CompleteComment = lastCompletionComments.Get(this.interviewId);
            this.CompleteCommentLabel = UIResources.Interview_Complete_Note_For_Supervisor;
        }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }

        public string EntitiesWithErrorsDescription { get; private set; }

        public IList<EntityWithErrorsViewModel> EntitiesWithErrors { get; private set; }

        private IMvxAsyncCommand completeInterviewCommand;
        public IMvxAsyncCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ?? 
                    (this.completeInterviewCommand = new MvxAsyncCommand(async () => await this.CompleteInterviewAsync(), () => !WasThisInterviewCompleted));
            }
        }

        public string CompleteComment
        {
            get => completeComment;
            set
            {
                completeComment = value;
                this.lastCompletionComments.Store(this.interviewId, value);
            }
        }

        public string CompleteCommentLabel { get; protected set; }

        private bool wasThisInterviewCompleted = false;
        public bool WasThisInterviewCompleted
        {
            get => this.wasThisInterviewCompleted;
            set => this.RaiseAndSetIfChanged(ref this.wasThisInterviewCompleted, value);
        }

        private string completeComment;

        private async Task CompleteInterviewAsync()
        {
            if (this.WasThisInterviewCompleted)
                return;

            this.WasThisInterviewCompleted = true;
            await this.commandService.WaitPendingCommandsAsync();

            var completeInterview = new CompleteInterviewCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                comment: this.CompleteComment,
                completeTime: DateTime.UtcNow);

            try
            {
                await this.commandService.ExecuteAsync(completeInterview);
                this.lastCompletionComments.Remove(interviewId);

                await this.CloseInterviewAfterComplete();
            }
            catch (InterviewException e)
            {
                logger.Warn("Interview has unexpected status", e);
            }
        }

        protected virtual async Task CloseInterviewAfterComplete()
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.interviewId.ToString());

            this.messenger.Publish(new InterviewCompletedMessage(this));
        }

        public void Dispose()
        {
            this.Name.Dispose();
            this.InterviewState.DisposeIfDisposable();
        }
    }
}
