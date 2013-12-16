using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "UpdateGroup")]
    public class UpdateGroupCommand : FullGroupDataCommand
    {
        public UpdateGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, Guid? rosterSizeQuestionId, string description, string condition, bool isRoster,
            RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles)
            : base(
                questionnaireId, groupId, responsibleId, title, rosterSizeQuestionId, description, condition, isRoster, rosterSizeSource,
                rosterFixedTitles) {}
    }
}