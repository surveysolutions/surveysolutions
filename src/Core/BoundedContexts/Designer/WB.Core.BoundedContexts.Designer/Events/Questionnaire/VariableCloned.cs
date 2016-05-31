using System;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class VariableCloned : QuestionnaireVariableEvent
    {
        public VariableCloned(Guid entityId, Guid responsibleId, Guid parentId, Guid? sourceQuestionnaireId, Guid sourceEntityId, int targetIndex, VariableData variableData)
            : base(entityId, responsibleId, variableData)
        {
            this.ParentId = parentId;
            this.SourceQuestionnaireId = sourceQuestionnaireId;
            this.SourceEntityId = sourceEntityId;
            this.TargetIndex = targetIndex;
        }

        public Guid ParentId { get; set; }
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceEntityId { get; set; }
        public int TargetIndex { get; set; }
    }
}