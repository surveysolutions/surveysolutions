using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    public class CloneStaticTextCommand : QuestionnaireEntityCloneCommand
    {
        public CloneStaticTextCommand(Guid questionnaireId, Guid entityId, Guid responsibleId, Guid sourceEntityId)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId,
                sourceEntityId: sourceEntityId)
        {
        }
    }
}