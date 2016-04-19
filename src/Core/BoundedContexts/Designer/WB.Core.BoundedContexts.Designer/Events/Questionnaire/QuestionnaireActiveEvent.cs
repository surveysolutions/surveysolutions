using System;
using WB.Core.Infrastructure.EventBus;

namespace Main.Core.Events.Questionnaire
{
    public abstract class QuestionnaireActiveEvent : IEvent
    {
        public Guid ResponsibleId { get; set; }

        protected QuestionnaireActiveEvent() {}

        protected QuestionnaireActiveEvent(Guid responsibleId)
        {
            this.ResponsibleId = responsibleId;
        }
    }
}