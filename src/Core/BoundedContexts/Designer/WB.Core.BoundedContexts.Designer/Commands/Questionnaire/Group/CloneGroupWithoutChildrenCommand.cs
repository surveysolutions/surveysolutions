using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    public class CloneGroupWithoutChildrenCommand : FullGroupDataCommand
    {
        public CloneGroupWithoutChildrenCommand(Guid questionnaireId, Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string condition,
            Guid? parentGroupId, Guid sourceGroupId, int targetIndex, bool isRoster, RosterSizeSourceType rosterSizeSource,
            Tuple<decimal,string>[] fixedRosterTitles, Guid? rosterTitleQuestionId)
            : base(
                questionnaireId, groupId, responsibleId, title, variableName, rosterSizeQuestionId, condition, isRoster, rosterSizeSource,
                fixedRosterTitles, rosterTitleQuestionId)
        {
            this.ParentGroupId = parentGroupId;
            this.SourceGroupId = sourceGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid? ParentGroupId { get; private set; }
        public Guid SourceGroupId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}