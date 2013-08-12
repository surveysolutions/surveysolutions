namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "SetComment")]
    public class SetCommentCommand : CommandBase
    {
        public SetCommentCommand(
            Guid completeQuestionnaireId,
            Guid questionPublicKey,
            string comments,
            Guid? propogationPublicKey,
            UserLight user)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;

            this.User = user;
            this.QuestionPublickey = questionPublicKey;
            this.PropogationPublicKey = propogationPublicKey;
            this.Comments = comments;
        }

        public UserLight User { get; set; }

        public string Comments { get; set; }

        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public Guid? PropogationPublicKey { get; set; }

        public Guid QuestionPublickey { get; set; }
    }
}