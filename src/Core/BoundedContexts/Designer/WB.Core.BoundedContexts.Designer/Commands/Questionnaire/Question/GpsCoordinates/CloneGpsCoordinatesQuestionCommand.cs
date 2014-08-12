using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.GpsCoordinates
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CloneGpsCoordinatesQuestion")]
    public class CloneGpsCoordinatesQuestionCommand : AbstractCloneQuestionCommand
    {
        public CloneGpsCoordinatesQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid parentGroupId,
            Guid sourceQuestionId,
            int targetIndex,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            QuestionScope scope)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions,
                parentGroupId: parentGroupId,
                sourceQuestionId: sourceQuestionId, targetIndex: targetIndex,variableLabel:variableLabel)
        {
            this.Scope = scope;
        }

        public QuestionScope Scope { get; set; }
    }
}