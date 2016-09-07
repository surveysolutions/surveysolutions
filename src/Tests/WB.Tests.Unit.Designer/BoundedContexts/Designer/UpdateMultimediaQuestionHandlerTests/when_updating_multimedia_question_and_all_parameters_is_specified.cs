using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultimediaQuestionHandlerTests
{
    internal class when_updating_multimedia_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                questionType : QuestionType.Text,
                publicKey : questionId,
                groupPublicKey : chapterId,
                questionText : "old title",
                stataExportCaption : "old_variable_name",
                instructions : "old instructions",
                conditionExpression : "old condition",
                responsibleId : responsibleId
            ));
            eventContext = new EventContext();
        };

        Because of = () =>
                questionnaire.UpdateMultimediaQuestion(questionId: questionId, title: "title",
                    variableName: "multimedia_question",
                    variableLabel: variableName, enablementCondition: condition, hideIfDisabled: hideIfDisabled, instructions: instructions,
                    responsibleId: responsibleId, scope: QuestionScope.Interviewer, properties: Create.QuestionProperties());

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_MultimediaQuestionUpdated_event = () =>
            eventContext.ShouldContainEvent<MultimediaQuestionUpdated>();

        It should_raise_MultimediaQuestionUpdated_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .QuestionId.ShouldEqual(questionId);

        It should_raise_MultimediaQuestionUpdated_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .VariableName.ShouldEqual(variableName);

        It should_raise_MultimediaQuestionUpdated_event_with_title_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .Title.ShouldEqual(title);

        It should_raise_MultimediaQuestionUpdated_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .EnablementCondition.ShouldEqual(condition);

        It should_raise_MultimediaQuestionUpdated_event_with_hideIfDisabled_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .HideIfDisabled.ShouldEqual(hideIfDisabled);

        It should_raise_MultimediaQuestionUpdated_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>()
                .Instructions.ShouldEqual(instructions);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "multimedia_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
        private static bool hideIfDisabled = true;
    }
}
