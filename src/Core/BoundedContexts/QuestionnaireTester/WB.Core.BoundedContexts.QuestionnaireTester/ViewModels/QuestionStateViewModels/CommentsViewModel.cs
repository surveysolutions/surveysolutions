using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels
{
    public class CommentsViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswerCommented>
    {
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;

        public CommentsViewModel(IStatefullInterviewRepository interviewRepository, ILiteEventRegistry eventRegistry)
        {
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");
            if (eventRegistry == null) throw new ArgumentNullException("eventRegistry");

            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
        }

        private string interviewId;
        private Identity entityIdentity;

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;

            HasComments = false;

            this.eventRegistry.Subscribe(this);
        }

        private bool hasComments;
        public bool HasComments
        {
            get { return this.hasComments; }
            private set { this.hasComments = value; this.RaisePropertyChanged(); }
        }

        public void Handle(AnswerCommented @event)
        {
            if (@event.QuestionId != entityIdentity.Id || @event.PropagationVector.SequenceEqual(entityIdentity.RosterVector))
                return;
        }

        public void Dispose()
        {
            eventRegistry.Unsubscribe(this);
        }
    }
}