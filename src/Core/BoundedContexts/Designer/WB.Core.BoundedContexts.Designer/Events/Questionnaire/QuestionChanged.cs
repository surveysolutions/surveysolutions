namespace Main.Core.Events.Questionnaire
{
    using System;
    using Ncqrs.Eventing.Storage;

    public class QuestionChanged : FullQuestionDataEvent
    {
        public Guid TargetGroupKey { get; set; }
    }
}