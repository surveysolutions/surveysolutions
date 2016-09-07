using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateDateTimeQuestionHandlerTests
{
    internal class when_updating_datetime_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(
                Create.Event.NewQuestionAdded(
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

        Because of = () => questionnaire.UpdateDateTimeQuestion(command);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionChanged_event_with_specified_properties = () =>
        {
            var changedEvent = eventContext.GetSingleEvent<QuestionChanged>();

            changedEvent.PublicKey.ShouldEqual(questionId);
            changedEvent.StataExportCaption.ShouldEqual(variableName);
            changedEvent.QuestionText.ShouldEqual(title);
            changedEvent.ConditionExpression.ShouldEqual(enablementCondition);
            changedEvent.Instructions.ShouldEqual(instructions);
            changedEvent.Featured.ShouldEqual(isPreFilled);
            changedEvent.QuestionScope.ShouldEqual(scope);
            changedEvent.ValidationConditions.First().Expression.ShouldEqual(validationExpression);
            changedEvent.ValidationConditions.First().Message.ShouldEqual(validationMessage);
            changedEvent.IsTimestamp.ShouldEqual(isTimestamp);
        };

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
        private static bool isTimestamp = true;
        
        private static readonly UpdateDateTimeQuestion command = new UpdateDateTimeQuestion(
            questionnaireId: Guid.Parse("22222222222222222222222222222222"),
            questionId: questionId,
            isPreFilled: isPreFilled,
            scope: scope,
            responsibleId: responsibleId,
            validationConditions:
                new System.Collections.Generic.List<ValidationCondition>
                {
                    new ValidationCondition {Message = validationMessage, Expression = validationExpression}
                },
            commonQuestionParameters: new CommonQuestionParameters
            {
                Title = title,
                VariableName = variableName,
                VariableLabel = null,
                EnablementCondition = enablementCondition,
                HideIfDisabled = false,
                Instructions = instructions
            },
            isTimestamp: isTimestamp);
    }
}