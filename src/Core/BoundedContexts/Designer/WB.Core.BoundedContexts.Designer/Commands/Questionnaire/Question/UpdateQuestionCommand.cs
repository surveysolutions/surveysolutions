using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "UpdateQuestion")]
    public class UpdateQuestionCommand : FullQuestionDataCommand
    {
        public UpdateQuestionCommand(
            Guid questionnaireId, 
            Guid questionId,
            string title, 
            QuestionType type,
            string variableName,
            string variableLabel,
            string mask,
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
            int? maxAllowedAnswers, 
            bool? isFilteredCombobox,
            Guid? cascadeFromQuestionId)
            : base(questionnaireId, questionId, title, type, variableName, variableLabel,mask, isMandatory, isPreFilled,
                scope, enablementCondition, validationExpression, validationMessage, instructions, options, responsibleId,
                linkedToQuestionId, areAnswersOrdered, maxAllowedAnswers, isFilteredCombobox, cascadeFromQuestionId) 
        {}
    }
}