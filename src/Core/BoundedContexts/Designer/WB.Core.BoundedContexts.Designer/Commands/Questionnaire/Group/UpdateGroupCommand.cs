using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "NewUpdateGroup")]
    public class UpdateGroupCommand : FullGroupDataCommand
    {
        public UpdateGroupCommand(Guid questionnaireId, Guid groupId, string title, Propagate propagationKind, string description, string condition)
            : base(questionnaireId, groupId, title, propagationKind, description, condition) {}
    }
}