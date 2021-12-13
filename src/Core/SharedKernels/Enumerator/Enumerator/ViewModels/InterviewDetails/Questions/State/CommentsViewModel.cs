using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public enum CommentState
    {
        OwnComment = 0,
        ResolvedComment = 1,
        OtherUserComment = 2
    }

    public class CommentsViewModel : MvxNotifyPropertyChanged,
        IViewModelEventHandler<AnswerCommentResolved>,
        ICompositeEntity,
        IDisposable
    {
        public event EventHandler<EventArgs> CommentsInputShown;

        public class CommentViewModel
        {
            public string Comment { get; set; }

            public string CommentCaption { get; set; }

            public Identity Identity { get; set; }

            public CommentState CommentState { get; set; }
            public UserRoles UserRole { get; set; }
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ICommandService commandService;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;
        private readonly IPrincipal principal;

        protected CommentsViewModel() { }

        public CommentsViewModel(
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            ICommandService commandService,
            IViewModelEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.principal = principal;
            this.commandService = commandService;
            this.eventRegistry = eventRegistry;
            this.mvxMainThreadDispatcher = mvxMainThreadDispatcher;
        }

        private string interviewId;
        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            this.Identity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.GetOrThrow(interviewId);

            var question = interview.GetQuestion(entityIdentity);
            if (question != null)
            {
                var questionComments = interview.GetQuestionComments(this.Identity, true);
                var anyResolvedCommentsExists = questionComments.Any(x => x.Resolved);
                this.ShowResolvedCommentsVisible = anyResolvedCommentsExists;
                this.ShowResolvedComments = false;
            }
            this.HasComments = !string.IsNullOrWhiteSpace(this.InterviewerComment);

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void UpdateCommentsFromInterview(bool showResolved = false)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var allQuestionComments = interview.GetQuestionComments(this.Identity, true) ?? new List<AnswerComment>();
            var visibleComments = allQuestionComments.Where(x => x.Resolved == showResolved).ToList();

            this.ShowResolvedCommentsVisible = allQuestionComments.Any(x => x.Resolved);
            this.Comments.Clear();
            visibleComments.Select(this.ToViewModel).ForEach(x => this.Comments.Add(x));
            ShowHideResolveButton();
        }

        private void ShowHideResolveButton()
        {
            var principalCurrentUserIdentity = this.principal.CurrentUserIdentity;
            bool anyHqComment = this.Comments.Any(x => x.CommentState != CommentState.ResolvedComment
                                                        && x.UserRole == UserRoles.Headquarter || x.UserRole == UserRoles.Administrator);

            var interview = this.interviewRepository.GetOrThrow(interviewId);
            this.ResolveCommentsButtonVisible = principalCurrentUserIdentity.UserId == interview.SupervisorId
                                                && this.Comments.Any(x => x.CommentState != CommentState.ResolvedComment)
                                                && !anyHqComment;
        }

        private CommentViewModel ToViewModel(AnswerComment comment)
        {
            var isCurrentUserComment = comment.UserId == this.principal.CurrentUserIdentity.UserId;
            var commentCaption = GetCommentCaption(comment);

            var onPreviousAnswer = comment.CommentOnPreviousAnswer;
            if (onPreviousAnswer)
                commentCaption = $"{commentCaption} ({UIResources.Interview_Comment_OnPreviousAnswer})"; 

            CommentState commentState;
            if (comment.Resolved)
            {
                commentState = CommentState.ResolvedComment;
            }
            else
            {
                commentState = isCurrentUserComment ? CommentState.OwnComment : CommentState.OtherUserComment;
            }

            return new CommentViewModel
            {
                Comment = comment.Comment,
                CommentCaption = commentCaption,
                Identity = this.Identity,
                CommentState = commentState,
                UserRole = comment.UserRole,
            };
        }

        private string GetCommentCaption(AnswerComment comment)
        {
            var isCurrentUserComment = comment.UserId == this.principal.CurrentUserIdentity.UserId;

            if (isCurrentUserComment)
            {
                return UIResources.Interview_Comment_Interviewer_Caption;
            }

            switch (comment.UserRole)
            {
                case UserRoles.Headquarter:
                    return UIResources.Interview_Headquarters_Comment_Caption;
                case UserRoles.Supervisor:
                    return UIResources.Interview_Supervisor_Comment_Caption;
                case UserRoles.Interviewer:
                    return UIResources.Interview_Interviewer_Comment_Caption;
                default:
                    return UIResources.Interview_Other_Comment_Caption;
            }
        }

        public int MaxTextLength => AnswerUtils.TextAnswerMaxLength;

        public string ShowResolvedCommentsBtnText
        {
            get => showResolvedCommentsBtnText;
            set => SetProperty(ref showResolvedCommentsBtnText, value);
        }

        public bool ShowResolvedCommentsVisible
        {
            get => showResolvedCommentsVisible;
            private set => SetProperty(ref showResolvedCommentsVisible, value);
        }

        public bool ResolveCommentsButtonVisible
        {
            get => resolveCommentsButtonVisible;
            private set => SetProperty(ref resolveCommentsButtonVisible, value);
        }

        private bool ShowResolvedComments
        {
            get => showResolvedComments;
            set
            {
                if (value)
                {
                    ShowResolvedCommentsBtnText = UIResources.Interview_Question_HideResolvedComments;
                }
                else
                {
                    ShowResolvedCommentsBtnText = UIResources.Interview_Question_ShowResolvedComments;
                }

                UpdateCommentsFromInterview(value);
                this.showResolvedComments = value;
            }
        }

        private bool hasComments;
        public bool HasComments
        {
            get { return this.hasComments; }
            set { this.hasComments = value; this.RaisePropertyChanged(); }
        }

        private bool isCommentInEditMode;
        public bool IsCommentInEditMode
        {
            get { return this.isCommentInEditMode; }
            private set
            {
                this.isCommentInEditMode = value;
                this.HasComments = value || !string.IsNullOrWhiteSpace(this.interviewerComment);
                this.RaisePropertyChanged();
            }
        }

        private string interviewerComment;
        private string showResolvedCommentsBtnText;
        private bool showResolvedComments;
        private bool resolveCommentsButtonVisible;
        private bool showResolvedCommentsVisible;

        public string InterviewerComment
        {
            get { return this.interviewerComment; }
            set
            {
                if (this.interviewerComment != value)
                {
                    this.interviewerComment = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<CommentViewModel> Comments { get; } = new ObservableCollection<CommentViewModel>();
        public IMvxAsyncCommand InterviewerCommentChangeCommand => new MvxAsyncCommand(async () =>
            await this.SendCommentQuestionCommandAsync(), () => this.principal.IsAuthenticated);

        public IMvxCommand ToggleShowResolvedComments => new MvxCommand(() =>
        {
            this.ShowResolvedComments = !this.ShowResolvedComments;
        });

        public IMvxAsyncCommand ResolveComments => new MvxAsyncCommand(async () =>
        {
            await this.commandService.ExecuteAsync(
                    new ResolveCommentAnswerCommand(
                        interviewId: Guid.Parse(this.interviewId),
                        userId: this.principal.CurrentUserIdentity.UserId,
                        questionId: this.Identity.Id,
                        rosterVector: this.Identity.RosterVector))
                .ConfigureAwait(false);
        });

        private async Task SendCommentQuestionCommandAsync()
        {
#if !PRODUCTION
            if (this.InterviewerComment?.Equals("!{kaboom}!", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                throw new Exception("Test exception");
            }
#endif
            await this.commandService.ExecuteAsync(
                new CommentAnswerCommand(
                    interviewId: Guid.Parse(this.interviewId),
                    userId: this.principal.CurrentUserIdentity.UserId,
                    questionId: this.Identity.Id,
                    rosterVector: this.Identity.RosterVector,
                    comment: this.InterviewerComment))
                .ConfigureAwait(false);

            await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(() => UpdateCommentsFromInterview());

            ShowHideResolveButton();
            this.InterviewerComment = "";
            this.IsCommentInEditMode = false;
        }

        public void ShowCommentInEditor()
        {
            this.OnCommentsInputShown();
            this.IsCommentInEditMode = true;
        }

        protected virtual void OnCommentsInputShown() => this.CommentsInputShown?.Invoke(this, EventArgs.Empty);

        public void Handle(AnswerCommentResolved @event)
        {
            if (@event.QuestionId == this.Identity.Id &&
                @event.RosterVector.Identical(this.Identity.RosterVector))
            {
                this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(() => UpdateCommentsFromInterview(this.showResolvedComments));
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
        }
    }
}
