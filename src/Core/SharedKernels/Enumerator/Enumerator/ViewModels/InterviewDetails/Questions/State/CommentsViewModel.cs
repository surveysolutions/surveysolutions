using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
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

            public UserRoles UserRole { get; set; }
        }

        private readonly IStatefulInterviewRepository interviewRepository;

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

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.questionIdentity = entityIdentity;

            var interview = this.interviewRepository.Get(interviewId);
            var comments = interview.GetQuestionComments(entityIdentity) ?? new List<QuestionComment>();

            comments.Select(ToViewModel).ForEach(x => Comments.Add(x));

            this.HasComments = !string.IsNullOrWhiteSpace(this.InterviewerComment);
        }

        private CommentViewModel ToViewModel(QuestionComment comment)
        {
            var isCurrentUserComment = this.principal.CurrentUserIdentity.UserId == comment.UserId;
            var isAuthorityComment = this.authorituesRoles.Contains(comment.UserRole);

            return new CommentViewModel
            {
                Comment = comment.Comment,
                UserRole = comment.UserRole,
                CommentCaption = isCurrentUserComment 
                ? UIResources.Interview_Comment_Interviewer_Caption
                : (isAuthorityComment
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

        private readonly HashSet<UserRoles> authorituesRoles = new HashSet<UserRoles>{ UserRoles.Administrator, UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Supervisor};

        private IMvxCommand valueChangeCommand;
        public IMvxCommand InterviewerCommentChangeCommand
        {
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand(async () => await this.SendCommentQuestionCommandAsync())); }
        }

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

            if (!string.IsNullOrWhiteSpace(this.InterviewerComment))
                Comments.Add(ToViewModel(new QuestionComment(this.InterviewerComment, this.principal.CurrentUserIdentity.UserId, UserRoles.Operator)));

            this.InterviewerComment = "";
            this.IsCommentInEditMode = false;
        }

        public void ShowCommentInEditor()
        {
            this.OnCommentsInputShown();
            this.IsCommentInEditMode = true;
        }

        protected virtual void OnCommentsInputShown()
        {
            this.CommentsInputShown?.Invoke(this, EventArgs.Empty);
        }
    }
}