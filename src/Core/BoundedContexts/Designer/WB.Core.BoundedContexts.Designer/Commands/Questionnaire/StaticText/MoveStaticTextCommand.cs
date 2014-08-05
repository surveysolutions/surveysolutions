using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "MoveStaticText")]
    public class MoveStaticTextCommand : QuestionnaireEntityMoveCommand
    {
        public MoveStaticTextCommand(Guid questionnaireId, Guid entityId, Guid targetEntityId, int targetIndex,
            Guid responsibleId)
            : base(
                questionnaireId: questionnaireId, targetEntityId: targetEntityId, entityId: entityId,
                responsibleId: responsibleId, targetIndex: targetIndex)
        {
        }
    }
}