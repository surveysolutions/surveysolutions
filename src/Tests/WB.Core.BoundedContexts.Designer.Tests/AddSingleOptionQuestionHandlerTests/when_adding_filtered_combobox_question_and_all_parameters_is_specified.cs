using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.AddSingleOptionQuestionHandlerTests
{
    internal class when_adding_filtered_combobox_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            eventContext = new EventContext();
        };

        private Because of = () =>
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

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private It should_raise_NewQuestionAdded_event = () =>
            eventContext.ShouldContainEvent<NewQuestionAdded>();

        private It should_raise_NewQuestionAdded_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .PublicKey.ShouldEqual(questionId);

        private It should_raise_NewQuestionAdded_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .GroupPublicKey.ShouldEqual(chapterId);

        private It should_raise_NewQuestionAdded_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .StataExportCaption.ShouldEqual(variableName);

        private It should_raise_NewQuestionAdded_event_with_title_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .QuestionText.ShouldEqual(title);

        private It should_raise_NewQuestionAdded_event_with_enablementCondition_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .ConditionExpression.ShouldEqual(enablementCondition);

        private It should_raise_NewQuestionAdded_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Mandatory.ShouldEqual(isMandatory);

        private It should_raise_NewQuestionAdded_event_with_isfeatured_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Featured.ShouldEqual(isPreFilled);

        private It should_raise_NewQuestionAdded_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .Instructions.ShouldEqual(instructions);

        private It should_raise_NewQuestionAdded_event_with_nullable_answers = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>().Answers.ShouldBeNull();

        private It should_raise_NewQuestionAdded_event_with_linkedToQuestionId_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .LinkedToQuestionId.ShouldEqual(linkedToQuestionId);

        private It should_raise_NewQuestionAdded_event_with_isFilteredCombobox_specified = () =>
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

        private static Option[] options = new Option[]
        { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };

        private static Guid? linkedToQuestionId = (Guid?) null;
        private static Guid? cascadeFromQuestionId = (Guid?) null;
        private static bool isFilteredCombobox = true;
    }
}