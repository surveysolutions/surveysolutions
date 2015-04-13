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
            Guid? parentGroupId, bool isRoster, RosterSizeSourceType rosterSizeSource, Tuple<string, string>[] fixedRosterTitles, Guid? rosterTitleQuestionId,
            int? index = null)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, condition, isRoster, rosterSizeSource,
                fixedRosterTitles, rosterTitleQuestionId)
        {
            this.ParentGroupId = parentGroupId;
            this.Index = index;
        }

        public Guid? ParentGroupId { get; private set; }

        public int? Index { get; private set; }
    }
}