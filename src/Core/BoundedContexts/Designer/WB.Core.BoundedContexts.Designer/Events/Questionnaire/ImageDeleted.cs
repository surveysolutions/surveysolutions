namespace Main.Core.Events.Questionnaire
{
    using System;

    public class ImageDeleted: QuestionnaireActiveEvent
    {
        public Guid ImageKey { get; set; }

        public Guid QuestionKey { get; set; }
    }
}