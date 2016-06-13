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
        ILitePublishedEventHandler<AnswerRemoved>,
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
            if (interviewId == null) throw new ArgumentNullException("interviewId");

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
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questions == null) throw new ArgumentNullException("questions");

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
            var shouldNotifyAboutInterviewAnswer = this.reactOnAllInterviewEvents && @event.EventSourceId.FormatGuid() == this.interviewId;
            var shouldNotifyAboutListOfAnswers = this.questions.Length > 0 && 
                                                 this.questions.Any(x => @event.Payload.QuestionId == x.Id && @event.Payload.RosterVector.Identical(x.RosterVector));

            var shouldRaiseEvent = shouldNotifyAboutInterviewAnswer || shouldNotifyAboutListOfAnswers;

            if (shouldRaiseEvent)
            {
                this.OnSomeQuestionAnswered();
            }
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<PictureQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<TextListQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        protected virtual void OnSomeQuestionAnswered()
        {
            var handler = this.QuestionAnswered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Handle(IPublishedEvent<AnswerRemoved> @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Dispose()
        {
            this.registry.Unsubscribe(this);
        }
    }
}