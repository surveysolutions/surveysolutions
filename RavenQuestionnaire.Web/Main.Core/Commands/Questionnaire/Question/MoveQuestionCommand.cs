namespace Main.Core.Commands.Questionnaire.Question
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "MoveQuestion")]
    public class MoveQuestionCommand : QuestionCommand
    {
        public MoveQuestionCommand(Guid questionnaireId, Guid questionId, Guid targetGroupId, int targetIndex)
            : base(questionnaireId, questionId)
        {
            this.TargetGroupId = targetGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid TargetGroupId { get; set; }
        public int TargetIndex { get; set; }
    }
}