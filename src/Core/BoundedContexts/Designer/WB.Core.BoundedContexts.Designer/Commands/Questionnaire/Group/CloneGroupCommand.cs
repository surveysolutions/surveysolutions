using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
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