using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class AnswerNotifier :
        ILiteEventHandler<TextQuestionAnswered>,
        ILiteEventHandler<DateTimeQuestionAnswered>,
        ILiteEventHandler<GeoLocationQuestionAnswered>,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        ILiteEventHandler<NumericIntegerQuestionAnswered>,
        ILiteEventHandler<NumericRealQuestionAnswered>,
        ILiteEventHandler<PictureQuestionAnswered>,
        ILiteEventHandler<QRBarcodeQuestionAnswered>,
        ILiteEventHandler<SingleOptionLinkedQuestionAnswered>,
        ILiteEventHandler<SingleOptionQuestionAnswered>,
        ILiteEventHandler<TextListQuestionAnswered>,
        ILiteEventHandler<AnswerRemoved>,
        ILiteEventHandler<YesNoQuestionAnswered>,
        IDisposable
    {
        private readonly ILiteEventRegistry registry;
        private Guid? questionId;
        private Identity[] questions = { };
        string interviewId;

        public event EventHandler QuestionAnswered;

        protected AnswerNotifier() {}

        public AnswerNotifier(ILiteEventRegistry registry)
        {
            this.registry = registry;
        }

        public virtual void Init(string interviewId, Guid questionId)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");

            if (this.interviewId != null)
            {
                this.registry.Unsubscribe(this, this.interviewId);
            }

            this.interviewId = interviewId;
            this.questionId = questionId;
            this.registry.Subscribe(this, interviewId);
        }

        public virtual void Init(string interviewId, params Identity[] questions)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questions == null) throw new ArgumentNullException("questions");

            if (this.interviewId != null)
            {
                this.registry.Unsubscribe(this, this.interviewId);
            }

            this.interviewId = interviewId;
            this.questions = questions;
            this.registry.Subscribe(this, interviewId);
        }

        private void RaiseEventIfNeeded(QuestionActiveEvent @event)
        {
            var shouldNotifyAboutSingleAnswer = this.questionId.HasValue && @event.QuestionId == this.questionId;
            var shouldNotifyAboutListOfAnswers = this.questions.Length > 0 && 
                                                 this.questions.Any(x => @event.QuestionId == x.Id && @event.RosterVector.Identical(x.RosterVector));

            var shouldRaiseEvent = shouldNotifyAboutSingleAnswer || shouldNotifyAboutListOfAnswers;

            if (shouldRaiseEvent)
            {
                this.OnSomeQuestionAnswered();
            }
        }

        public void Handle(TextQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(DateTimeQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(GeoLocationQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(NumericIntegerQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(NumericRealQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(PictureQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(QRBarcodeQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(SingleOptionLinkedQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(SingleOptionQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(TextListQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Handle(YesNoQuestionAnswered @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        protected virtual void OnSomeQuestionAnswered()
        {
            var handler = this.QuestionAnswered;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Handle(AnswerRemoved @event)
        {
            this.RaiseEventIfNeeded(@event);
        }

        public void Dispose()
        {
            this.registry.Unsubscribe(this, interviewId);
        }
    }
}