using System;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels
{
    public class CommentsViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel
    {
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

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.questionIdentity = entityIdentity;

            var interview = this.interviewRepository.Get(interviewId);
            InterviewerComment = interview.GetInterviewerAnswerComment(entityIdentity);

            HasComments = !string.IsNullOrWhiteSpace(InterviewerComment);
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
                if (interviewerComment != value)
                {
                    interviewerComment = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand InterviewerCommentChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(async () => await this.SendCommentQuestionCommand())); }
        }


        private async Task SendCommentQuestionCommand()
        {
            await commandService.ExecuteAsync(
                new CommentAnswerCommand(
                    interviewId: Guid.Parse(interviewId),
                    userId: principal.CurrentUserIdentity.UserId,
                    questionId: this.questionIdentity.Id,
                    rosterVector: this.questionIdentity.RosterVector,
                    commentTime: DateTime.UtcNow,
                    comment: InterviewerComment));

            IsCommentInEditMode = false;
        }

        public void ShowCommentInEditor()
        {
            IsCommentInEditMode = true;
        }
    }
}