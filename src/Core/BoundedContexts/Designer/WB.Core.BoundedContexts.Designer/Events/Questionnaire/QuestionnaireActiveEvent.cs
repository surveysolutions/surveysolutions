using System;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Main.Core.Events.Questionnaire
{
    public abstract class QuestionnaireActiveEvent : ILiteEvent
    {
        public Guid ResponsibleId { get; set; }

        protected QuestionnaireActiveEvent() {}

        protected QuestionnaireActiveEvent(Guid responsibleId)
        {
            this.ResponsibleId = responsibleId;
        }
    }
}