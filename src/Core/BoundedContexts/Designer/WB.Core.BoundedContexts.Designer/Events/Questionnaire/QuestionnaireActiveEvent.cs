using System;

namespace Main.Core.Events.Questionnaire
{
    public abstract class QuestionnaireActiveEvent
    {
        public Guid ResponsibleId { get; set; }
    }
}