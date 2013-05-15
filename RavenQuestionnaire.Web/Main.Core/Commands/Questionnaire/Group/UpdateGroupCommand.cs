using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewUpdateGroup")]
    public class UpdateGroupCommand : FullGroupDataCommand
    {
        public UpdateGroupCommand(Guid questionnaireId, Guid groupId, string title, Propagate propagationKind, string description, string condition)
            : base(questionnaireId, groupId, title, propagationKind, description, condition) {}
    }
}