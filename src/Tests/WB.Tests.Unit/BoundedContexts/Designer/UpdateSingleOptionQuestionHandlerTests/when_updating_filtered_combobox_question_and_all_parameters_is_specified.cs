﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_filtered_combobox_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddSingleOptionQuestion(
                questionId: questionId,
                parentGroupId: chapterId,
                title: title,
                variableName: "old_variable_name",
                variableLabel: null,
                isMandatory: false,
                isPreFilled: false,
                scope: QuestionScope.Supervisor,
                enablementCondition: "",
                validationExpression: "",
                validationMessage: "",
                instructions: "",
                responsibleId: responsibleId,
                options: old_options,
                linkedToQuestionId: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: null);
            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateSingleOptionQuestion(
                questionId: questionId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isMandatory: isMandatory,
                isPreFilled: isPreFilled,
                scope: scope,
                enablementCondition: enablementCondition,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId,
                options: new_options,
                linkedToQuestionId: linkedToQuestionId,
                isFilteredCombobox: isFilteredCombobox,
                cascadeFromQuestionId: сascadeFromQuestionId);

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionChanged_event = () =>
            eventContext.ShouldContainEvent<QuestionChanged>();

        It should_raise_QuestionChanged_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_QuestionChanged_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .StataExportCaption.ShouldEqual(variableName);

        It should_raise_QuestionChanged_event_with_title_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .QuestionText.ShouldEqual(title);

        It should_raise_QuestionChanged_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_raise_QuestionChanged_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Mandatory.ShouldEqual(isMandatory);

        It should_raise_QuestionChanged_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Instructions.ShouldEqual(instructions);

        It should_raise_QuestionChanged_event_with_featured_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Featured.ShouldEqual(isPreFilled);

        It should_raise_QuestionChanged_event_with_scope_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .QuestionScope.ShouldEqual(scope);

        It should_raise_QuestionChanged_event_with_validationExpression_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .ValidationExpression.ShouldEqual(validationExpression);

        It should_raise_QuestionChanged_event_with_validationMessage_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .ValidationMessage.ShouldEqual(validationMessage);

        It should_raise_QuestionChanged_event_with_isFilteredCombobox_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .IsFilteredCombobox.ShouldEqual(isFilteredCombobox);

        It should_raise_NewQuestionAdded_event_with_same_options_count_as_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Answers.Length.ShouldEqual(old_options.Length);

        It should_raise_NewQuestionAdded_event_with_same_option_titles_as_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Answers.Select(x => x.AnswerText).ShouldContainOnly(old_options.Select(x => x.Title));

        It should_raise_NewQuestionAdded_event_with_same_option_values_as_specified = () =>
           eventContext.GetSingleEvent<QuestionChanged>()
               .Answers.Select(x => x.AnswerValue).ShouldContainOnly(old_options.Select(x => x.Value));


        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "some condition";
        private static string validationExpression = "some validation";
        private static string validationMessage = "validation message";
        private static Option[] old_options = new Option[] { new Option(Guid.NewGuid(), "1", "Option old 1"), new Option(Guid.NewGuid(), "2", "Option old 2"), };
        private static Option[] new_options = new Option[] { new Option(Guid.NewGuid(), "3", "Option 1"), new Option(Guid.NewGuid(), "4", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = true;
        private static Guid? сascadeFromQuestionId = (Guid?)null;
    }
}