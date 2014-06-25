using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "NewUpdateQuestion")]
    public class UpdateQuestionCommand : FullQuestionDataCommand
    {
        public UpdateQuestionCommand(
            Guid questionnaireId, 
            Guid questionId,
            string title, 
            QuestionType type,
            string variableName, string variableLabel,
            bool isMandatory, 
            bool isPreFilled,
            QuestionScope scope, 
            string enablementCondition,
            string validationExpression, 
            string validationMessage, 
            string instructions,
            Option[] options,
            Guid responsibleId,
            Guid? linkedToQuestionId,
            bool areAnswersOrdered, 
            int? maxAllowedAnswers)

            : base(questionnaireId, questionId, title, type, variableName, variableLabel, isMandatory, isPreFilled,
                scope, enablementCondition, validationExpression, validationMessage, instructions, options, responsibleId, linkedToQuestionId, areAnswersOrdered, maxAllowedAnswers) { }
    }
}