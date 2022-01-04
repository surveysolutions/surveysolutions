using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    public class UpdateGroup : FullGroupDataCommand
    {
        public UpdateGroup(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition, bool hideIfDisabled, bool isRoster,
            RosterSizeSourceType rosterSizeSource, FixedRosterTitleItem[] fixedRosterTitles,
            Guid? rosterTitleQuestionId, 
            RosterDisplayMode displayMode)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, condition, hideIfDisabled, isRoster, rosterSizeSource,
                fixedRosterTitles, rosterTitleQuestionId, displayMode) {}
    }
}
