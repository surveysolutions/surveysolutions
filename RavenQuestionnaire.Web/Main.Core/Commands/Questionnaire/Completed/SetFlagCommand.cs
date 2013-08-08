namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The set comment command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(_CompleteQuestionnaireAR), "SetFlag")]
    public class _SetFlagCommand : CommandBase
    {
        public _SetFlagCommand(
            Guid completeQuestionnaireId,
            Guid questionPublicKey,
            Guid? propogationPublicKey,
            bool isFlaged)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;

            //// this.Executor = executor;
            this.QuestionPublickey = questionPublicKey;
            this.PropogationPublicKey = propogationPublicKey;
            this.IsFlaged = isFlaged;
        }
        
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public Guid? PropogationPublicKey { get; set; }

        public Guid QuestionPublickey { get; set; }
        
        public bool IsFlaged { get; set; }
    }
}