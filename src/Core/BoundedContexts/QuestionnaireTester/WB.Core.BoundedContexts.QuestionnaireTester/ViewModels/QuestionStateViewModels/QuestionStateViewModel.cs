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
        ILiteEventBusEventHandler<TAnswerEvent>
        where TAnswerEvent : QuestionAnswered
    {
        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }
        public CommentsViewModel Comments { get; private set; }

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefullInterviewRepository interviewRepository;
        private Identity entityIdentity;
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

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.entityIdentity = entityIdentity;
            this.interviewId = interviewId;

            liteEventRegistry.Subscribe(this);

            this.Header.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity);
            this.Comments.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);
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
            IsAnswered = interview.WasAnswered(entityIdentity);
        }

        public void ExecutedAnswerCommandWithoutExceptions()
        {
            Validity.ExecutedWithoutExceptions();
        }

        public void ProcessAnswerCommandException(Exception exception)
        {
            Validity.ProcessException(exception);
        }
    }
}