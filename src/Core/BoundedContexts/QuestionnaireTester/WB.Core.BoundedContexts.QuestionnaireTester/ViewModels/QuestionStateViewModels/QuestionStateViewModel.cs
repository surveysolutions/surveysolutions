using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels
{
    public class QuestionStateViewModel<TAnswerEvent>: MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<TAnswerEvent>
        where TAnswerEvent : QuestionAnswered
    {
        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }
        public CommentsViewModel Comments { get; private set; }

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefullInterviewRepository interviewRepository;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel(
            ILiteEventRegistry liteEventRegistry,
            IStatefullInterviewRepository interviewRepository,
            ValidityViewModel validityViewModel, 
            QuestionHeaderViewModel questionHeaderViewModel,
            EnablementViewModel enablementViewModel,
            CommentsViewModel commentsViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            Validity = validityViewModel;
            Header = questionHeaderViewModel;
            Enablement = enablementViewModel;
            Comments = commentsViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            liteEventRegistry.Subscribe(this);

            this.Header.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity, navigationState);
            this.Comments.Init(interviewId, entityIdentity, navigationState);
            this.Enablement.Init(interviewId, entityIdentity, navigationState);
        }       
        
        private bool isAnswered;
        public bool IsAnswered
        {
            get { return isAnswered; }
            set { isAnswered = value; RaisePropertyChanged(); }
        }

        public void Handle(TAnswerEvent @event)
        {
            var interview = this.interviewRepository.Get(interviewId);
            IsAnswered = interview.WasAnswered(questionIdentity);
        }

        public void ExecutedAnswerCommandWithoutExceptions()
        {
            Validity.ExecutedWithoutExceptions();
        }

        public void ProcessAnswerCommandException(Exception exception)
        {
            Validity.ProcessException(exception);
        }

        public void ShowCommentInEditor()
        {
            Comments.ShowCommentInEditor();
        }
    }
}