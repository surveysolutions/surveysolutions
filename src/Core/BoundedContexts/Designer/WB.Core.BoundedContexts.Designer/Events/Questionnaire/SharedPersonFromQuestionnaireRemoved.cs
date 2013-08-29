namespace Main.Core.Events.Questionnaire
{
    using System;

    public class SharedPersonFromQuestionnaireRemoved : QuestionnaireActiveEvent
    {
        public Guid PersonId { get; set; }
    }
}