using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    internal class when_updating_text_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey: questionId,
                groupPublicKey: chapterId,
                questionText: "old title",
                stataExportCaption: "old_variable_name",
                instructions: "old instructions",
                conditionExpression: "old condition",
                responsibleId: responsibleId,
                questionType: QuestionType.QRBarcode
                ));
            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateTextQuestion(
                questionId: questionId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isPreFilled: isPreFilled,
                scope: scope,
                enablementCondition: enablementCondition,
                hideIfDisabled: false,
                instructions: instructions,
                 mask: null,
                responsibleId: responsibleId, validationCoditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>
                {
                    new ValidationCondition { Expression = validationExpression, Message = validationMessage }
                }, properties: Create.QuestionProperties());

        Cleanup stuff = () =>
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
                .ValidationConditions.First().Expression.ShouldEqual(validationExpression);

        It should_raise_QuestionChanged_event_with_validationMessage_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>()
               .ValidationConditions.First().Message.ShouldEqual(validationMessage);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "some condition";
        private static string validationExpression = "some validation";
        private static string validationMessage = "validation message";
    }
}