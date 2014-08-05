using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "CloneStaticText")]
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