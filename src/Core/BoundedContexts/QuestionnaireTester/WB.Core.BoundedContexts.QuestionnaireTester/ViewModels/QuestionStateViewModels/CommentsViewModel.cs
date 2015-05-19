using System;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels
{
    public class CommentsViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel
    {
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        public CommentsViewModel(
            IStatefullInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            IPrincipal principal,
            ICommandService commandService)
        {
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");
            if (eventRegistry == null) throw new ArgumentNullException("eventRegistry");

            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.principal = principal;
            this.commandService = commandService;
        }

        private string interviewId;
        private Identity entityIdentity;

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;

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

                    SendCommentQuestionCommand();
                }
            }
        }

        private void SendCommentQuestionCommand()
        {
            Task.Run(() => SendCommentQuestionCommandImpl());
        }

        private void SendCommentQuestionCommandImpl()
        {
            commandService.Execute(
                new CommentAnswerCommand(
                    interviewId: Guid.Parse(interviewId),
                    userId: principal.CurrentUserIdentity.UserId,
                    questionId: this.entityIdentity.Id,
                    rosterVector: this.entityIdentity.RosterVector,
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