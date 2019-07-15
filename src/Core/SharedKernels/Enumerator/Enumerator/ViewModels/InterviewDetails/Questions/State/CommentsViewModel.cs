using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
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
        ILiteEventHandler<AnswerCommentResolved>,
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
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private IStatefulInterview interview;
        private readonly ICommandService commandService;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IPrincipal principal;

        protected CommentsViewModel() { }

        public CommentsViewModel(
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            ICommandService commandService,
            ILiteEventRegistry eventRegistry)
        {
            this.interviewRepository = interviewRepository;
            this.principal = principal;
            this.commandService = commandService;
            this.eventRegistry = eventRegistry;
        }

        private string interviewId;
        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.eventRegistry.Subscribe(this, interviewId);
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            this.Identity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));

            this.interview = this.interviewRepository.Get(interviewId);

            var questionComments = interview.GetQuestionComments(this.Identity, true);
            var anyResolvedCommentsExists = questionComments.Any(x => x.Resolved);
            this.ShowResolvedCommentsVisible = anyResolvedCommentsExists;
            ShowResolvedComments = false;
            this.HasComments = !string.IsNullOrWhiteSpace(this.InterviewerComment);
        }

        private void UpdateCommentsFromInterview(bool showResolved = false)
        {
            var comments = interview.GetQuestionComments(this.Identity, showResolved) ?? new List<AnswerComment>();
            this.Comments.Clear();
            comments.Select(this.ToViewModel).ForEach(x => this.Comments.Add(x));
            ShowHideResolveButton();
        }

        private void ShowHideResolveButton()
        {
            this.ResolveCommentsButtonVisible = this.principal.CurrentUserIdentity.UserId == this.interview.SupervisorId
                                                && this.Comments.Any(x => x.CommentState != CommentState.ResolvedComment);
        }

        private CommentViewModel ToViewModel(AnswerComment comment)
        {
            var isCurrentUserComment = comment.UserId == this.principal.CurrentUserIdentity.UserId;
            var commentCaption = GetCommentCaption(comment);

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
                CommentState = commentState
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

        public bool ShowResolvedCommentsVisible { get; private set; }

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
            await this.commandService.ExecuteAsync(
                new CommentAnswerCommand(
                    interviewId: Guid.Parse(this.interviewId),
                    userId: this.principal.CurrentUserIdentity.UserId,
                    questionId: this.Identity.Id,
                    rosterVector: this.Identity.RosterVector,
                    comment: this.InterviewerComment))
                .ConfigureAwait(false);

            await this.InvokeOnMainThreadAsync(() => UpdateCommentsFromInterview());

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
                InvokeOnMainThread(() => UpdateCommentsFromInterview(this.showResolvedComments));
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
        }
    }
}
