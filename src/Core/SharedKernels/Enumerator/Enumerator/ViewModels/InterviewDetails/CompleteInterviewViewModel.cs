using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CompleteInterviewViewModel : MvxViewModel, IDisposable
    {
        protected readonly IViewModelNavigationService viewModelNavigationService;
        
        private readonly ICommandService commandService;
        private readonly IEntitiesListViewModelFactory entitiesListViewModelFactory;
        private readonly ILastCompletionComments lastCompletionComments;
        protected readonly IPrincipal principal;

        
        protected readonly IMvxMessenger Messenger;
        
        public InterviewStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }
        public string CompleteScreenTitle { get; set; }

        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal,
            IEntitiesListViewModelFactory entitiesListViewModelFactory,
            ILastCompletionComments lastCompletionComments,
            InterviewStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel,
            ILogger logger)
        {
            Messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.entitiesListViewModelFactory = entitiesListViewModelFactory;
            this.lastCompletionComments = lastCompletionComments;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
            this.logger = logger;
        }

        protected readonly ILogger logger;

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

            this.Comment = lastCompletionComments.Get(this.interviewId);
            this.CommentLabel = UIResources.Interview_Complete_Note_For_Supervisor;
        }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }

        public string EntitiesWithErrorsDescription { get; private set; }

        public bool CanSwitchToWebMode
        {
            get => canSwitchToWebMode;
            set => this.RaiseAndSetIfChanged(ref this.canSwitchToWebMode, value);
        }

        public bool RequestWebInterview
        {
            get => requestWebInterview;
            set => this.RaiseAndSetIfChanged(ref this.requestWebInterview, value);
        }

        public string WebInterviewUrl { get; set; }

        public IList<EntityWithErrorsViewModel> EntitiesWithErrors { get; private set; }

        private IMvxAsyncCommand completeInterviewCommand;
        public IMvxAsyncCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ??= new MvxAsyncCommand(async () =>
                    await this.CompleteInterviewAsync(), () => !WasThisInterviewCompleted);
            }
        }

        public string Comment
        {
            get => comment;
            set
            {
                comment = value;
                this.lastCompletionComments.Store(this.interviewId, value);
            }
        }

        public string CommentLabel { get; protected set; }

        private bool wasThisInterviewCompleted = false;
        public bool WasThisInterviewCompleted
        {
            get => this.wasThisInterviewCompleted;
            set => this.RaiseAndSetIfChanged(ref this.wasThisInterviewCompleted, value);
        }

        private string comment;
        private bool requestWebInterview;
        private bool canSwitchToWebMode;

        private async Task CompleteInterviewAsync()
        {
            if (this.WasThisInterviewCompleted)
                return;

            this.WasThisInterviewCompleted = true;
            await this.commandService.WaitPendingCommandsAsync();

            ICommand completeInterview = this.RequestWebInterview
            ? new ChangeInterviewModeCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                InterviewMode.CAWI,
                comment: this.Comment)
            : new CompleteInterviewCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                comment: this.Comment);

            try
            {
                await this.commandService.ExecuteAsync(completeInterview);
            }
            catch (InterviewException e)
            {
                logger.Warn("Interview has unexpected status", e);
            }

            this.lastCompletionComments.Remove(interviewId);
            await this.CloseInterviewAfterComplete(this.RequestWebInterview);
        }

        protected virtual async Task CloseInterviewAfterComplete(bool switchInterviewToCawiMode)
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync(this.interviewId.ToString());
            Dispose();
            Messenger.Publish(new InterviewCompletedMessage(this));
        }

        public void Dispose()
        {
            this.Name.Dispose();
            var entitiesWithErrors = this.EntitiesWithErrors;
            foreach (var entityWithErrorsViewModel in entitiesWithErrors)
            {
                entityWithErrorsViewModel.DisposeIfDisposable();
            }
            this.InterviewState.DisposeIfDisposable();
        }
    }
}
