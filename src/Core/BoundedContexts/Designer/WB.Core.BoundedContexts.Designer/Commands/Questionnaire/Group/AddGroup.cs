using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    public class AddGroup : FullGroupDataCommand
    {
        public AddGroup(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition, bool hideIfDisabled,
            Guid? parentGroupId, bool isRoster, RosterDisplayMode displayMode, RosterSizeSourceType rosterSizeSource, FixedRosterTitleItem[] fixedRosterTitles, Guid? rosterTitleQuestionId,
            int? index = null)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, condition, hideIfDisabled, isRoster, rosterSizeSource,
                fixedRosterTitles, rosterTitleQuestionId, displayMode)
        {
            this.ParentGroupId = parentGroupId;
            this.Index = index;
        }

        public Guid? ParentGroupId { get; private set; }

        public int? Index { get; private set; }
    }
}
