using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class QuestionStateViewModel<TAnswerEvent>: ReadonlyQuestionStateViewModel,
        IViewModelEventHandler<TAnswerEvent>
        where TAnswerEvent : QuestionAnswered
    {
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly AnswersRemovedNotifier answersRemovedNotifier;
        private Identity questionIdentity;
        private string interviewId;

        protected QuestionStateViewModel() { }

        public QuestionStateViewModel(
            IViewModelEventRegistry liteEventRegistry,
            IStatefulInterviewRepository interviewRepository,
            ValidityViewModel validityViewModel, 
            QuestionHeaderViewModel questionHeaderViewModel,
            EnablementViewModel enablementViewModel,
            CommentsViewModel commentsViewModel,
            AnswersRemovedNotifier answersRemovedNotifier,
            WarningsViewModel warningsViewModel)
            : base(validityViewModel, questionHeaderViewModel, enablementViewModel, commentsViewModel, warningsViewModel)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.answersRemovedNotifier = answersRemovedNotifier;
        }
        
        public override void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionIdentity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));

            base.Init(interviewId, entityIdentity, navigationState);
            
            var interview = this.interviewRepository.Get(interviewId);
            this.IsAnswered = interview.WasAnswered(entityIdentity);

            this.answersRemovedNotifier.Init(interviewId, entityIdentity);
            this.answersRemovedNotifier.AnswerRemoved += this.AnswerRemoved;

            this.Enablement.EntityEnabled += this.EnablementOnEntityEnabled;

            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        private void AnswerRemoved(object sender, EventArgs eventArgs)
        {
            this.UpdateFromModel();
        }

        private void EnablementOnEntityEnabled(object sender, EventArgs eventArgs)
        {
            this.UpdateFromModel();
        }

        private bool isAnswered;
        public bool IsAnswered
        {
            get => this.isAnswered;
            set => SetProperty(ref this.isAnswered, value);
        }

        public void Handle(TAnswerEvent @event)
        {
            if (this.questionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                this.UpdateFromModel();
            }
        }

        private void UpdateFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            this.IsAnswered = interview.WasAnswered(this.questionIdentity);
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.liteEventRegistry.Unsubscribe(this);
                this.Enablement.EntityEnabled -= this.EnablementOnEntityEnabled;
                this.answersRemovedNotifier.AnswerRemoved -= this.AnswerRemoved;
                this.answersRemovedNotifier.Dispose();
            }
        }
    }
}
