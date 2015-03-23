using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.AddTextQuestion(
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
                mask:null,
                responsibleId: responsibleId,
                index: index);

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

        It should_raise_NewQuestionAdded_event_with_title_specified = () =>
            eventContext.GetSingleEvent<NewQuestionAdded>()
                .QuestionText.ShouldEqual(title);

        It should_raise_NewQuestionAdded_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .StataExportCaption.ShouldEqual(variableName);

        It should_raise_NewQuestionAdded_event_with_enablementCondition_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_raise_NewQuestionAdded_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Mandatory.ShouldEqual(isMandatory);

        It should_raise_NewQuestionAdded_event_with_ifeatured_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Featured.ShouldEqual(isPreFilled);

        It should_raise_NewQuestionAdded_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
                .Instructions.ShouldEqual(instructions);

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_index_specified = () =>
           eventContext.GetSingleEvent<QuestionnaireItemMoved>()
               .TargetIndex.ShouldEqual(index);

        It should_raise_QuestionnaireItemMoved_event_with_GroupKey_specified = () =>
           eventContext.GetSingleEvent<QuestionnaireItemMoved>()
               .GroupKey.ShouldEqual(chapterId);

        It should_raise_QuestionnaireItemMoved_event_with_PublicKey_specified = () =>
          eventContext.GetSingleEvent<QuestionnaireItemMoved>()
              .PublicKey.ShouldEqual(questionId);

        It should_raise_QuestionnaireItemMoved_event_with_ResponsibleId_specified = () =>
          eventContext.GetSingleEvent<QuestionnaireItemMoved>()
              .ResponsibleId.ShouldEqual(responsibleId);

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
        private static int index = 5;
    }
}