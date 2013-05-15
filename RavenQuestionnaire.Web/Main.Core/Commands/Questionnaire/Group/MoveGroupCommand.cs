using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "MoveGroup")]
    public class MoveGroupCommand : GroupCommand
    {
        public MoveGroupCommand(Guid questionnaireId, Guid groupId, Guid? targetGroupId, int targetIndex)
            : base(questionnaireId, groupId)
        {
            this.TargetGroupId = targetGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid? TargetGroupId { get; set; }
        public int TargetIndex { get; set; }
    }
}