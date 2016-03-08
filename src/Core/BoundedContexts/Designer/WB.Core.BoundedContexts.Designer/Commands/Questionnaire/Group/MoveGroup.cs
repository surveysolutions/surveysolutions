using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    public class MoveGroup : GroupCommand
    {
        public MoveGroup(Guid questionnaireId, Guid groupId, Guid? targetGroupId, int targetIndex, Guid responsibleId)
            : base(questionnaireId, groupId, responsibleId)
        {
            this.TargetGroupId = targetGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid? TargetGroupId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}