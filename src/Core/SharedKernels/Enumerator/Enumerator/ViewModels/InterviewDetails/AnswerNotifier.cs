using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class AnswerNotifier :
        ILitePublishedEventHandler<TextQuestionAnswered>,
        ILitePublishedEventHandler<DateTimeQuestionAnswered>,
        ILitePublishedEventHandler<GeoLocationQuestionAnswered>,
        ILitePublishedEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        ILitePublishedEventHandler<MultipleOptionsQuestionAnswered>,
        ILitePublishedEventHandler<NumericIntegerQuestionAnswered>,
        ILitePublishedEventHandler<NumericRealQuestionAnswered>,
        ILitePublishedEventHandler<PictureQuestionAnswered>,
        ILitePublishedEventHandler<QRBarcodeQuestionAnswered>,
        ILitePublishedEventHandler<SingleOptionLinkedQuestionAnswered>,
        ILitePublishedEventHandler<SingleOptionQuestionAnswered>,
        ILitePublishedEventHandler<TextListQuestionAnswered>,
        ILitePublishedEventHandler<AnswersRemoved>,
        ILitePublishedEventHandler<YesNoQuestionAnswered>,
        IDisposable
    {
        private readonly ILiteEventRegistry registry;
        private Identity[] questions = { };
        private bool reactOnAllInterviewEvents = false;
        string interviewId;

        public virtual event EventHandler QuestionAnswered;

        protected AnswerNotifier() {}

        public AnswerNotifier(ILiteEventRegistry registry)
        {
            this.registry = registry;
        }

        public virtual void Init(string interviewId) 
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));

            if (this.interviewId != null)
            {
                this.registry.Unsubscribe(this);
            }

            this.interviewId = interviewId;
            this.reactOnAllInterviewEvents = true;
            this.registry.Subscribe(this, interviewId);
        }

        public virtual void Init(string interviewId, params Identity[] questions)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questions == null) throw new ArgumentNullException(nameof(questions));

            if (this.interviewId != null)
            {
                this.registry.Unsubscribe(this);
            }

            this.interviewId = interviewId;
            this.questions = questions;
            this.registry.Subscribe(this, interviewId);
        }

        private void RaiseEventIfNeeded(IPublishedEvent<QuestionActiveEvent> @event)
        {
            var shouldRaiseEvent = this.ShouldRaiseEvent(@event.EventSourceId.FormatGuid(),
                new Identity(@event.Payload.QuestionId, @event.Payload.RosterVector));

            if (shouldRaiseEvent)
            {
                this.OnSomeQuestionAnswered();
            }
        }

        private bool ShouldRaiseEvent(string interviewId, Identity questionIdentity)
        {
            var shouldNotifyAboutInterviewAnswer = this.reactOnAllInterviewEvents && interviewId == this.interviewId;
            var shouldNotifyAboutListOfAnswers = this.questions.Length > 0 &&
                                                 this.questions.Any(x => x.Equals(questionIdentity));

            return shouldNotifyAboutInterviewAnswer || shouldNotifyAboutListOfAnswers;
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<PictureQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<TextListQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(IPublishedEvent<AnswersRemoved> @event)
        {
            string interviewId = @event.EventSourceId.FormatGuid();

            if (@event.Payload.Questions.Any(x =>
                this.ShouldRaiseEvent(interviewId, new Identity(x.Id, x.RosterVector))))
            {
                this.OnSomeQuestionAnswered();
            }
        }

        protected virtual void OnSomeQuestionAnswered() => this.QuestionAnswered?.Invoke(this, EventArgs.Empty);

        public void Dispose() => this.registry.Unsubscribe(this);
    }
}