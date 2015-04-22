using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    public class UpdateGroupCommand : FullGroupDataCommand
    {
        public UpdateGroupCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition, bool isRoster,
            RosterSizeSourceType rosterSizeSource, FixedRosterTitleItem[] fixedRosterTitles, Guid? rosterTitleQuestionId)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, condition, isRoster, rosterSizeSource,
                fixedRosterTitles, rosterTitleQuestionId) {}
    }
}