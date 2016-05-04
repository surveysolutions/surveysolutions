using System;
using System.Linq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class AnswersRemovedNotifier : 
        ILiteEventHandler,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<AnswerRemoved>,
        IDisposable
    {
        private readonly ILiteEventRegistry eventRegistry;
        private Identity Identity;
        private string interviewId;

        public event EventHandler AnswerRemoved;

        public AnswersRemovedNotifier(ILiteEventRegistry eventRegistry)
        {
            this.eventRegistry = eventRegistry;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.Identity = entityIdentity;
            this.interviewId = interviewId;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Any(x => this.Identity.Equals(x)))
            {
                this.OnAnswerRemoved();
            }
        }

        public void Handle(AnswerRemoved @event)
        {
            if (this.Identity.Equals(@event.QuestionId, @event.RosterVector))
            {
                this.OnAnswerRemoved();
            }
        }

        protected virtual void OnAnswerRemoved()
        {
            this.AnswerRemoved?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, this.interviewId);
        }
    }
}