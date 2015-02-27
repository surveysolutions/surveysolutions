using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

namespace WB.Tests.Unit
{
    public static class QuestionnaireExtensions
    {
        public static void AddTextQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName,
            string variableLabel,
            bool isMandatory,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            string mask,
            Guid responsibleId)
        {
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateTextQuestion(questionId, title, variableName, variableLabel, isMandatory, isPreFilled, scope, enablementCondition, validationExpression, validationMessage, instructions, mask, responsibleId);
        }

        public static void AddGpsCoordinatesQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName,
            string variableLabel,
            bool isMandatory,
            QuestionScope scope,
            string enablementCondition,
            string instructions,
            Guid responsibleId)
        {
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateGpsCoordinatesQuestion(questionId, title, variableName, variableLabel, isMandatory, scope, enablementCondition, string.Empty, string.Empty, instructions, responsibleId);
        }

        public static void AddDateTimeQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId)
        {
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateDateTimeQuestion(questionId, title, variableName, variableLabel, isMandatory, isPreFilled, scope, enablementCondition, validationExpression, validationMessage, instructions, responsibleId);
        }

        public static void AddMultiOptionQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId,
            Option[] options,
            Guid? linkedToQuestionId,
            bool areAnswersOrdered,
            int? maxAllowedAnswers)
        {
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateMultiOptionQuestion(questionId, title, variableName, variableLabel, isMandatory, scope, enablementCondition, validationExpression, validationMessage, instructions, responsibleId, options, linkedToQuestionId, areAnswersOrdered, maxAllowedAnswers);
        }

        public static void AddSingleOptionQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
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
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateSingleOptionQuestion(questionId, title, variableName, variableLabel, isMandatory, isPreFilled, scope, enablementCondition, validationExpression, validationMessage, instructions, responsibleId, options, linkedToQuestionId, isFilteredCombobox, cascadeFromQuestionId);
        }

        public static void AddNumericQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
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
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateDateTimeQuestion(questionId, title, variableName, variableLabel, isMandatory, isPreFilled, scope, enablementCondition, validationExpression, validationMessage, instructions, responsibleId);
        }

        public static void AddTextListQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName,
            string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            int? maxAnswerCount)
        {
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateTextListQuestion(questionId, title, variableName, variableLabel, isMandatory, enablementCondition, instructions, responsibleId, maxAnswerCount);
        }

        public static void AddQRBarcodeQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            string title,
            string variableName,
            string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string instructions,
            Guid responsibleId)
        {
            questionnaire.AddDefaultTypeQuestion(new AddDefaultTypeQuestionCommand(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateQRBarcodeQuestion(questionId, title, variableName, variableLabel, isMandatory, enablementCondition, instructions,responsibleId);
        }
    }
}