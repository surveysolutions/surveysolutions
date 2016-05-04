namespace Main.Core.Events.Questionnaire
{
    using System;

    public class QuestionnaireUpdated : QuestionnaireActiveEvent
    {
        [Obsolete]
        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}