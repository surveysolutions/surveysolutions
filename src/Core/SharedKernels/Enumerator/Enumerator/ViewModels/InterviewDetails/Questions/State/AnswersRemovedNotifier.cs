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
        IDisposable
    {
        private readonly ILiteEventRegistry eventRegistry;
        private Identity Identity;

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
            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Any(x => this.Identity.Equals(x)))
            {
                this.AnswerRemoved?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
        }
    }
}