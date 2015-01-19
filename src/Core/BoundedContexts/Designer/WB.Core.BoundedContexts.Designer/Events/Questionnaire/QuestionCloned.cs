using System;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Questionnaire
{
    [EventName("RavenQuestionnaire.Core:Events:QuestionCloned")]
    public class QuestionCloned : FullQuestionDataEvent
    {
        public Guid SourceQuestionId { get; set; }

        public int TargetIndex { get; set; }

        /// <summary>
        /// Gets or sets count of allowed answers for list question
        /// </summary>
        public int? MaxAnswerCount { get; set; }

        public int? CountOfDecimalPlaces { get; set; }
    }
}