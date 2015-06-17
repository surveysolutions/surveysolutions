using System;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
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
        ILiteEventHandler<TextListQuestionAnswered>
    {
        private readonly ILiteEventRegistry registry;
        private Guid questionId;

        protected AnswerNotifier()
        {
        }

        public event EventHandler QuestionAnswered;

        public AnswerNotifier(ILiteEventRegistry registry)
        {
            this.registry = registry;
        }

        public void Init(Guid questionId)
        {
            this.registry.Subscribe(this);
            this.questionId = questionId;
        }

        private void RaiseEventIfNeeded(QuestionAnswered @event)
        {
            if (@event.QuestionId == this.questionId)
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

        protected virtual void OnSomeQuestionAnswered()
        {
            var handler = this.QuestionAnswered;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}