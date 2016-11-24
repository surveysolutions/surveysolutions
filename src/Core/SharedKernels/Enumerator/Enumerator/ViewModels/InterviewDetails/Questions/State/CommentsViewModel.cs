using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
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
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private IStatefulInterview interview;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        protected CommentsViewModel() { }

        public CommentsViewModel(
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            ICommandService commandService)
        {
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.interviewRepository = interviewRepository;
            this.principal = principal;
            this.commandService = commandService;
        }

        private string interviewId;
        private Identity questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.questionIdentity = entityIdentity;

            this.interview = this.interviewRepository.Get(interviewId);
            this.UpdateCommentsFromInterview();

            this.HasComments = !string.IsNullOrWhiteSpace(this.InterviewerComment);
        }

        private void UpdateCommentsFromInterview()
        {
            var comments = interview.GetQuestionComments(this.questionIdentity) ?? new List<AnswerComment>();

            comments.Select(this.ToViewModel).ForEach(x => this.Comments.Add(x));
        }

        private CommentViewModel ToViewModel(AnswerComment comment)
        {
            var userId = this.principal.CurrentUserIdentity.UserId;
            var isCurrentUserComment = userId == comment.UserId;
            var isSupervisorComment = comment.UserId != userId; // TODO: KP-8225 This is bad and won't work after reassign, but previous code was no better so anyway.

            return new CommentViewModel
            {
                Comment = comment.Comment,
                IsCurrentUserComment = isCurrentUserComment,
                CommentCaption = isCurrentUserComment 
                ? UIResources.Interview_Comment_Interviewer_Caption
                : (isSupervisorComment
                    ? UIResources.Interview_Supervisor_Comment_Caption
                    : UIResources.Interview_Other_Comment_Caption)
            };
        }

        private bool hasComments;
        public bool HasComments
        {
            get { return this.hasComments; }
            private set { this.hasComments = value; this.RaisePropertyChanged(); }
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
        public IMvxAsyncCommand InterviewerCommentChangeCommand => new MvxAsyncCommand(this.SendCommentQuestionCommandAsync);

        private async Task SendCommentQuestionCommandAsync()
        {
            await this.commandService.ExecuteAsync(
                new CommentAnswerCommand(
                    interviewId: Guid.Parse(this.interviewId),
                    userId: this.principal.CurrentUserIdentity.UserId,
                    questionId: this.questionIdentity.Id,
                    rosterVector: this.questionIdentity.RosterVector,
                    commentTime: DateTime.UtcNow,
                    comment: this.InterviewerComment));

            this.UpdateCommentsFromInterview();

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