using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class CommentsViewModel : MvxNotifyPropertyChanged,
        ICompositeEntity
    {
        public event EventHandler<EventArgs> CommentsInputShown; 

        public class CommentViewModel
        {
            public string Comment { get; set; }

            public string CommentCaption  { get; set; }

            public bool IsCurrentUserComment { get; set; }

            public Identity Identity { get; set; }
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private IStatefulInterview interview;
        private readonly ICommandService commandService;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        private readonly IPrincipal principal;

        protected CommentsViewModel() { }

        public CommentsViewModel(
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            ICommandService commandService,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher)
        {
            this.interviewRepository = interviewRepository;
            this.principal = principal;
            this.commandService = commandService;
            this.mainThreadDispatcher = mainThreadDispatcher;
        }

        private string interviewId;
        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            this.Identity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));

            this.interview = this.interviewRepository.Get(interviewId);
            this.UpdateCommentsFromInterview();

            this.HasComments = !string.IsNullOrWhiteSpace(this.InterviewerComment);
        }

        private async Task UpdateCommentsFromInterview()
        {
            var comments = interview.GetQuestionComments(this.Identity) ?? new List<AnswerComment>();

            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.Comments.Clear();
                comments.Select(this.ToViewModel).ForEach(x => this.Comments.Add(x));
            });
        }

        private CommentViewModel ToViewModel(AnswerComment comment)
        {
            var isCurrentUserComment = comment.UserId == this.principal.CurrentUserIdentity.UserId;
            var commentCaption = GetCommentCaption(comment);

            return new CommentViewModel
            {
                Comment = comment.Comment,
                IsCurrentUserComment = isCurrentUserComment,
                CommentCaption = commentCaption,
                Identity = this.Identity
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

        private async Task SendCommentQuestionCommandAsync()
        {
            await this.commandService.ExecuteAsync(
                new CommentAnswerCommand(
                    interviewId: Guid.Parse(this.interviewId),
                    userId: this.principal.CurrentUserIdentity.UserId,
                    questionId: this.Identity.Id,
                    rosterVector: this.Identity.RosterVector,
                    comment: this.InterviewerComment)).ConfigureAwait(false);

            await this.UpdateCommentsFromInterview();

            this.InterviewerComment = "";
            this.IsCommentInEditMode = false;
        }

        public void ShowCommentInEditor()
        {
            this.OnCommentsInputShown();
            this.IsCommentInEditMode = true;
        }

        protected virtual void OnCommentsInputShown() => this.CommentsInputShown?.Invoke(this, EventArgs.Empty);
    }
}
