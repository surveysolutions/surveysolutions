using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "AddGroup")]
    public class AddGroupCommand : FullGroupDataCommand
    {
        public AddGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, Guid? rosterSizeQuestionId, string description, string condition,
            Guid? parentGroupId, bool isRoster, RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles)
            : base(
                questionnaireId, groupId, responsibleId, title, rosterSizeQuestionId, description, condition, isRoster, rosterSizeSource,
                rosterFixedTitles)
        {
            this.ParentGroupId = parentGroupId;
        }

        public Guid? ParentGroupId { get; private set; }
    }
}