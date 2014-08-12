using System;

namespace Main.Core.Events.Questionnaire
{
    public abstract class QuestionnaireEntityEvent : QuestionnaireActiveEvent
    {
        public Guid EntityId { get; set; }
    }
}