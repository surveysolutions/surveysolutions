using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class AnswerNotifier :
        IViewModelEventHandler<TextQuestionAnswered>,
        IViewModelEventHandler<DateTimeQuestionAnswered>,
        IViewModelEventHandler<GeoLocationQuestionAnswered>,
        IViewModelEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IViewModelEventHandler<MultipleOptionsQuestionAnswered>,
        IViewModelEventHandler<NumericIntegerQuestionAnswered>,
        IViewModelEventHandler<NumericRealQuestionAnswered>,
        IViewModelEventHandler<PictureQuestionAnswered>,
        IViewModelEventHandler<QRBarcodeQuestionAnswered>,
        IViewModelEventHandler<SingleOptionLinkedQuestionAnswered>,
        IViewModelEventHandler<SingleOptionQuestionAnswered>,
        IViewModelEventHandler<TextListQuestionAnswered>,
        IViewModelEventHandler<AnswersRemoved>,
        IViewModelEventHandler<YesNoQuestionAnswered>,
        IViewModelEventHandler<AreaQuestionAnswered>,
        IViewModelEventHandler<AudioQuestionAnswered>,
        IDisposable
    {
        private readonly IViewModelEventRegistry registry;
        private Identity[] questions = { };
        private bool reactOnAllInterviewEvents = false;
        string interviewId;

        public virtual event EventHandler QuestionAnswered;

        protected AnswerNotifier() {}

        public AnswerNotifier(IViewModelEventRegistry registry)
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

        public virtual void Init(string interviewId, Identity[] questions)
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

        private void RaiseEventIfNeeded(QuestionActiveEvent @event)
        {
            var shouldRaiseEvent = this.ShouldRaiseEvent(new Identity(@event.QuestionId, @event.RosterVector));

            if (shouldRaiseEvent)
            {
                this.OnSomeQuestionAnswered();
            }
        }

        private bool ShouldRaiseEvent(Identity questionIdentity)
        {
            var shouldNotifyAboutInterviewAnswer = this.reactOnAllInterviewEvents;
            var shouldNotifyAboutListOfAnswers = this.questions.Length > 0 &&
                                                 this.questions.Any(x => x.Equals(questionIdentity));

            return shouldNotifyAboutInterviewAnswer || shouldNotifyAboutListOfAnswers;
        }

        public void Handle(TextQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(DateTimeQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(GeoLocationQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(MultipleOptionsQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(NumericIntegerQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(NumericRealQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(PictureQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(QRBarcodeQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(SingleOptionLinkedQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(SingleOptionQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(TextListQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(YesNoQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(AreaQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(AudioQuestionAnswered @event) => this.RaiseEventIfNeeded(@event);

        public void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Any(x =>
                this.ShouldRaiseEvent(new Identity(x.Id, x.RosterVector))))
            {
                this.OnSomeQuestionAnswered();
            }
        }

        protected virtual void OnSomeQuestionAnswered() => this.QuestionAnswered?.Invoke(this, EventArgs.Empty);

        public void Dispose() => this.registry.Unsubscribe(this);
    }
}
