namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Domain;

    using Ncqrs;
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "SetAnswer")]
    public class SetAnswerCommand : CommandBase
    {
        public SetAnswerCommand(
            Guid completeQuestionnaireId,
            Guid questionPublicKey,
            List<Guid> сompleteAnswers,
            string сompleteAnswerValue,
            Guid? propogationPublicKey)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.PropogationPublicKey = propogationPublicKey;
            this.QuestionPublickey = questionPublicKey;
            this.CompleteAnswers = сompleteAnswers;
            this.CompleteAnswerValue = сompleteAnswerValue;
            var clock = NcqrsEnvironment.Get<IClock>();
            this.AnswerDate = clock.UtcNow();
        }

        public string CompleteAnswerValue { get; private set; }

        public List<Guid> CompleteAnswers { get; private set; }

        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public Guid? PropogationPublicKey { get; set; }

        public Guid QuestionPublickey { get; set; }

        public DateTime AnswerDate { get; set; }
    }
}