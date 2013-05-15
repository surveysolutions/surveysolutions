using Main.Core.Commands.Questionnaire.Base;

namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "NewAddGroup")]
    public class NewAddGroupCommand : FullGroupDataCommand
    {
        public NewAddGroupCommand(Guid questionnaireId, Guid groupId, Guid? parentGroupId, string title, Propagate propagationKind, string description, string condition)
            : base(questionnaireId, groupId, title, propagationKind, description, condition)
        {
            this.ParentGroupId = parentGroupId;
        }

        public Guid? ParentGroupId { get; set; }
    }
}