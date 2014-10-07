using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.AddSingleOptionQuestionHandlerTests
{
    internal class when_adding_single_option_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.AddSingleOptionQuestion(
                questionId: questionId,
                parentGroupId: chapterId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isMandatory: isMandatory,
                isPreFilled: isPreFilled,
                scope: QuestionScope.Interviewer,
                enablementCondition: enablementCondition,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId,
                options: options,
                linkedToQuestionId: linkedToQuestionId,
                cascadeFromQuestionId: cascadeFromQuestionId,
                isFilteredCombobox: isFilteredCombobox);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NewQuestionAdded_event = () =>
            eventContext.ShouldContainEvent<NewQuestionAdded>();

        It should_raise_NewQuestionAdded_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_NewQuestionAdded_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .GroupPublicKey.ShouldEqual(chapterId);

        It should_raise_NewQuestionAdded_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .StataExportCaption.ShouldEqual(variableName);

        It should_raise_NewQuestionAdded_event_with_title_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .QuestionText.ShouldEqual(title);

        It should_raise_NewQuestionAdded_event_with_enablementCondition_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_raise_NewQuestionAdded_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Mandatory.ShouldEqual(isMandatory);

        It should_raise_NewQuestionAdded_event_with_isfeatured_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Featured.ShouldEqual(isPreFilled);

        It should_raise_NewQuestionAdded_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Instructions.ShouldEqual(instructions);

        It should_raise_NewQuestionAdded_event_with_same_options_count_as_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Answers.Length.ShouldEqual(options.Length);

        It should_raise_NewQuestionAdded_event_with_same_option_titles_as_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Answers.Select(x => x.AnswerText).ShouldContainOnly(options.Select(x => x.Title));

        It should_raise_NewQuestionAdded_event_with_same_option_values_as_specified = () =>
           eventContext.GetSingleEvent<NewQuestionAdded>()
               .Answers.Select(x => x.AnswerValue).ShouldContainOnly(options.Select(x => x.Value));

        It should_raise_NewQuestionAdded_event_with_linkedToQuestionId_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .LinkedToQuestionId.ShouldEqual(linkedToQuestionId);

        It should_raise_NewQuestionAdded_event_with_isFilteredCombobox_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .IsFilteredCombobox.ShouldEqual(isFilteredCombobox);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "text_question";
        private static bool isMandatory = true;
        private static bool isPreFilled = false;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string enablementCondition = "enablementCondition";
        private static string validationExpression = "validationExpression";
        private static string validationMessage = "validationMessage";
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static Guid? cascadeFromQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = false;
    }
}