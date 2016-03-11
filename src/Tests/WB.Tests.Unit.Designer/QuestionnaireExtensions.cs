using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit
{
    internal static class QuestionnaireExtensions
    {
        public static void AddTextQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName,
            string variableLabel,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            string mask,
            Guid responsibleId,
            int? index = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId, index));
            var validationConditions = new List<ValidationCondition>().ConcatWithOldConditionIfNotEmpty(validationExpression, validationMessage);
            
            questionnaire.UpdateTextQuestion(
                questionId, 
                title, 
                variableName, 
                variableLabel, 
                isPreFilled, 
                scope, 
                enablementCondition, 
                false,
                instructions, 
                mask, 
                responsibleId, 
                validationConditions);
        }

        public static void AddMultiOptionQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId,
            Option[] options,
            Guid? linkedToQuestionId,
            bool areAnswersOrdered,
            int? maxAllowedAnswers,
            bool yesNoView)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateMultiOptionQuestion(questionId, title, variableName, variableLabel, scope, enablementCondition, false, instructions, responsibleId, options, linkedToQuestionId, areAnswersOrdered, maxAllowedAnswers, yesNoView, new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null);
        }

        public static void AddSingleOptionQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Option[] options,
            Guid? linkedToQuestionId,
            Guid responsibleId,
            bool isFilteredCombobox,
            Guid? cascadeFromQuestionId)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateSingleOptionQuestion(questionId, title, variableName, variableLabel, isPreFilled, scope, enablementCondition, false, instructions, responsibleId, options, linkedToQuestionId, isFilteredCombobox, cascadeFromQuestionId, new List<ValidationCondition>(),
                linkedFilterExpression: null);
        }

        public static void AddNumericQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            int? maxValue,
            Guid responsibleId,
            bool isInteger,
            int? countOfDecimalPlaces)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateDateTimeQuestion(
                questionId: questionId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isPreFilled: isPreFilled,
                scope: scope,
                enablementCondition: enablementCondition,
                hideIfDisabled: false,
                instructions: instructions,
                responsibleId: responsibleId,
                validationConditions: new List<ValidationCondition>()
                );
        }
    }
}