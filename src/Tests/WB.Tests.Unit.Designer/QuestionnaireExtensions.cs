using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer
{
    internal static class QuestionnaireExtensions
    {
        public static void AddTextQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentId,
            Guid responsibleId,
            string title = "title",
            string variableName = null,
            string variableLabel = null,
            bool isPreFilled = false,
            QuestionScope scope = QuestionScope.Interviewer,
            string enablementCondition =null ,
            string validationExpression = null,
            string validationMessage = null,
            string instructions= null,
            string mask = null,
            int? index = null,
            List<ValidationCondition>validationConditions = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(questionnaire.Id, questionId, parentId, title, responsibleId, index));
            var questionValidationConditions = validationConditions ?? new List<ValidationCondition>().ConcatWithOldConditionIfNotEmpty(validationExpression, 
                validationMessage).ToList();
            
            questionnaire.UpdateTextQuestion(new UpdateTextQuestion(questionnaire.Id,
                questionId, 
                responsibleId,
                new CommonQuestionParameters()
                {
                    Title = title, VariableName = variableName, VariableLabel = variableLabel,
                    EnablementCondition = enablementCondition, Instructions = instructions
                }, 
                mask,
                scope,
                isPreFilled,
                questionValidationConditions));
        }

        public static void AddMultiOptionQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            Guid responsibleId,
            Option[] options = null,
            string title = "title",
            string variableName = null, 
            string variableLabel = null,
            QuestionScope scope = QuestionScope.Interviewer,
            string enablementCondition = null,
            string validationExpression =null,
            string validationMessage = null,
            string instructions = null,
            Guid? linkedToQuestionId = null,
            bool areAnswersOrdered = false,
            int? maxAllowedAnswers = null,
            bool yesNoView = false,
            string optionsFilterExpression = null,
            string linkedFilterExpression = null,
            bool isFilteredCombobox = false,
            Guid? categoriesId = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            var questionProperties = Create.QuestionProperties();
            questionProperties.OptionsFilterExpression = optionsFilterExpression;
            var command = Create.Command.UpdateMultiOptionQuestion(
                questionId,
                responsibleId,
                title,
                variableName,
                variableLabel,
                enablementCondition,
                instructions,
                validationExpression,
                validationMessage,
                scope,
                options,
                linkedToQuestionId,
                areAnswersOrdered,
                maxAllowedAnswers,
                yesNoView,
                linkedFilterExpression,
                isFilteredCombobox,
                categoriesId: categoriesId);
            command.Properties = questionProperties;
            questionnaire.UpdateMultiOptionQuestion(command);
        }

        public static void AddSingleOptionQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            Guid responsibleId,
            Option[] options = null,
            string title = "title",
            string variableName = null, 
            string variableLabel = null,
            bool isPreFilled = false,
            QuestionScope scope = QuestionScope.Interviewer,
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            string instructions = null,
            Guid? linkedToQuestionId = null,
            bool isFilteredCombobox = false,
            Guid? cascadeFromQuestionId = null,
            bool showAsList = false,
            int? showAsListThreshold = null,
            Guid? categoriesId = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));

            var optionsList = options ?? new Option[] { new Option (title : "one", value : "1"), new Option (title : "two", value : "2" ) };

            questionnaire.UpdateSingleOptionQuestion(
                new UpdateSingleOptionQuestion(
                        questionnaireId: questionnaire.Id,
                        questionId: questionId,
                        commonQuestionParameters: new CommonQuestionParameters()
                        {
                            Title = title,
                            VariableName = variableName,
                            VariableLabel = null,
                            EnablementCondition = enablementCondition,
                            Instructions = instructions,
                            HideIfDisabled = false
                        },

                        isPreFilled: isPreFilled,
                        scope: scope,
                        responsibleId: responsibleId,
                        options: options,
                        linkedToEntityId: linkedToQuestionId,
                        isFilteredCombobox: isFilteredCombobox,
                        cascadeFromQuestionId: cascadeFromQuestionId,
                        validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                        linkedFilterExpression: null,
                        validationExpression: null,
                        validationMessage: null,
                        showAsList: showAsList,
                        showAsListThreshold: showAsListThreshold,
                        categoriesId: categoriesId));

            if (isFilteredCombobox || cascadeFromQuestionId.HasValue)
            {
                var question = questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId);
                question.Answers = optionsList.Select(x => new Answer
                {
                    AnswerText = x.Title,
                    AnswerValue = x.Value
                }).ToList();
            }
        }

        public static void AddNumericQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentId,
            Guid responsibleId,
            string title = "title",
            string variableName = null, 
            string variableLabel = null,
            bool isPreFilled = false,
            QuestionScope scope = QuestionScope.Interviewer,
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            string instructions = null,
            int? maxValue = null,
            bool isInteger = false,
            int? countOfDecimalPlaces = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(questionnaire.Id, questionId, parentId, title, responsibleId));
            questionnaire.UpdateNumericQuestion(new UpdateNumericQuestion(questionnaire.Id, questionId, responsibleId, 
                new CommonQuestionParameters()
                {
                    Title = title, VariableName = variableName, EnablementCondition = enablementCondition,
                    Instructions = instructions, VariableLabel=variableLabel
                    
                },
                isPreFilled, scope, isInteger, false, countOfDecimalPlaces, new List<ValidationCondition>(), null));
        }

        public static void AddGpsQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            Guid responsibleId,
            string title = "title",
            string variableName = null,
            string variableLabel = null,
            bool isPreFilled = false,
            QuestionScope scope = QuestionScope.Interviewer,
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            string instructions = null,
            int? maxValue = null,
            bool isInteger = false,
            int? countOfDecimalPlaces = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateGpsCoordinatesQuestion(new UpdateGpsCoordinatesQuestion(questionnaire.Id, questionId, responsibleId, 
                new CommonQuestionParameters() { Title = title, VariableName = variableName, VariableLabel = variableLabel,Instructions = instructions, EnablementCondition = enablementCondition},
                isPreFilled, validationExpression, validationMessage, scope, new List<ValidationCondition>()));
        }


        public static void AddQRBarcodeQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            Guid responsibleId,
            string title = "title",
            string variableName = null,
            string variableLabel = null,
            bool isPreFilled = false,
            QuestionScope scope = QuestionScope.Interviewer,
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            string instructions = null,
            int? maxValue = null,
            bool isInteger = false,
            int? countOfDecimalPlaces = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(Guid.NewGuid(), questionId, parentGroupId, title, responsibleId));
            questionnaire.UpdateQRBarcodeQuestion(new UpdateQRBarcodeQuestion(questionnaire.Id, questionId, responsibleId, new CommonQuestionParameters() { Title = title },
                validationExpression, validationMessage, scope, new List<ValidationCondition>()));
        }

        public static void AddTextListQuestion(
            this Questionnaire questionnaire,
            Guid questionId,
            Guid parentGroupId,
            Guid responsibleId,
            string title = "title",
            string variableName = null,
            string variableLabel = null,
            QuestionScope scope = QuestionScope.Interviewer,
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            string instructions = null,
            int? maxAnswerCount = null,
            int? index = null)
        {
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(new AddDefaultTypeQuestion(questionnaire.Id, questionId, parentGroupId, title, responsibleId, index));
            var validationConditions = new List<ValidationCondition>().ConcatWithOldConditionIfNotEmpty(validationExpression, validationMessage).ToList();

            questionnaire.UpdateTextListQuestion(new UpdateTextListQuestion(
                questionnaire.Id,
                questionId,
                responsibleId,
                new CommonQuestionParameters() { Title = title, VariableName = variableName, VariableLabel = variableLabel, EnablementCondition = enablementCondition },
                maxAnswerCount,
                scope,
                validationConditions));
        }

        public static void AddVariable(
            this Questionnaire questionnaire,
            Guid entityId,
            Guid parentId,
            Guid responsibleId,
            VariableType variableType = VariableType.String,
            string variableName = "variable",
            string variableExpression = null,
            bool doNotExport = false)
        {
            questionnaire.AddVariableAndMoveIfNeeded(
                new AddVariable(questionnaire.Id, 
                    entityId,
                    new VariableData(variableType, variableName, variableExpression, null, doNotExport), 
                    responsibleId, parentId));
            
        }

        public static void AddGroup(this Questionnaire questionnaire, 
            Guid? groupId,
            Guid? parentGroupId = null,
            Guid? responsibleId = null,
            string title = null,
            string variableName = null,
            bool isRoster = false,
            string enablingCondition = null,
            RosterSizeSourceType rosterSourceType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null, 
            FixedRosterTitleItem[] rosterFixedTitles = null)
        {
             questionnaire.AddGroupAndMoveIfNeeded(groupId ?? Guid.NewGuid(),
                 responsibleId ?? Guid.NewGuid(),
                 title ?? "Title - " + Guid.NewGuid().FormatGuid(),
                 variableName ?? "Variable_" + Guid.NewGuid().FormatGuid().Substring(0, 15),
                 rosterSizeQuestionId,
                 "Description - " + Guid.NewGuid().FormatGuid(),
                 enablingCondition,
                 false,
                 parentGroupId,
                 isRoster,
                 rosterSourceType,
                 rosterFixedTitles ?? (rosterSourceType == RosterSizeSourceType.FixedTitles ? new FixedRosterTitleItem[] {new FixedRosterTitleItem("1","1"),new FixedRosterTitleItem("2","2")}: new FixedRosterTitleItem[0]),
                 null,
                 null);
        }
        
        public static void AddStaticText(
            this Questionnaire questionnaire,
            Guid staticTextId,
            Guid parentGroupId,
            Guid responsibleId,
            string text = null,
            string enablingCondition = null,
            int? index = null)
        {
            questionnaire.AddStaticTextAndMoveIfNeeded(
                new AddStaticText(questionnaire.Id, 
                    staticTextId,
                    text, 
                    responsibleId,
                    parentGroupId,
                    index));
            
            questionnaire.UpdateStaticText(
                new UpdateStaticText(
                    questionnaire.Id,
                    staticTextId,
                    text,
                    String.Empty, 
                    responsibleId,
                    enablingCondition,
                    false,
                    new List<ValidationCondition>()));
        }
    }
}
