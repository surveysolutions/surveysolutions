using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "CloneGroup")]
    public class CloneGroupCommand : FullGroupDataCommand
    {
        public CloneGroupCommand(Guid questionnaireId, Guid groupId, Guid? parentGroupId, Guid sourceGroupId, int targetIndex,
            string title, Propagate propagationKind, string description, string condition)
            : base(questionnaireId, groupId, title, propagationKind, description, condition)
        {
            this.ParentGroupId = parentGroupId;
            this.SourceGroupId = sourceGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid? ParentGroupId { get; set; }
        public Guid SourceGroupId { get; set; }
        public int TargetIndex { get; set; }
    }
}