using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    public class AddGroupCommand : FullGroupDataCommand
    {
        public AddGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition,
            Guid? parentGroupId, bool isRoster, RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles, Guid? rosterTitleQuestionId)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, condition, isRoster, rosterSizeSource,
                rosterFixedTitles, rosterTitleQuestionId)
        {
            this.ParentGroupId = parentGroupId;
        }

        public Guid? ParentGroupId { get; private set; }
    }
}