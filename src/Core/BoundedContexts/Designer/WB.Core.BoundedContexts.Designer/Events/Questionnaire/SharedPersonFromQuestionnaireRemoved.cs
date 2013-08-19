namespace Main.Core.Events.Questionnaire
{
    using System;

    public class SharedPersonFromQuestionnaireRemoved
    {
        public Guid PersonId { get; set; }
        public string Email { get; set; }
    }
}