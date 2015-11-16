namespace Main.Core.Events.Questionnaire
{
    using System;

    public class ImageUpdated
    {
        public string Description { get; set; }

        public Guid ImageKey { get; set; }

        public Guid QuestionKey { get; set; }

        public string Title { get; set; }
    }
}