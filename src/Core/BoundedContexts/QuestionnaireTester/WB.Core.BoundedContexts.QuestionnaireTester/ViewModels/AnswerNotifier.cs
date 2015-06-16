using System;
using System.Diagnostics;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class AnswerNotifier : 
        ILiteEventHandler<TextQuestionAnswered>
    {
        private readonly ILiteEventRegistry registry;
        private Guid questionId;

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

        public void Handle(TextQuestionAnswered @event)
        {
            Debug.WriteLine("TextQuestionAnswered questionId {0}. RV: {1}, answer: {2} ", @event.QuestionId, string.Join(",", @event.PropagationVector), @event.Answer);
            if (@event.QuestionId == this.questionId)
            {
                this.OnSomeQuestionAnswered();
            }
        }

        protected virtual void OnSomeQuestionAnswered()
        {
            var handler = this.QuestionAnswered;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}