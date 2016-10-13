using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
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
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(
                    questionId,
                    chapterId,
                    responsibleId,
                    title: "old title",
                    variableName: "old_variable_name",
                    instructions: "old instructions",
                    enablementCondition: "old condition");
        };

        Because of = () => questionnaire.UpdateDateTimeQuestion(command);


        It should_raise_QuestionChanged_event_with_specified_properties = () =>
        {
            var question = questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId);

            question.PublicKey.ShouldEqual(questionId);
            question.StataExportCaption.ShouldEqual(variableName);
            question.QuestionText.ShouldEqual(title);
            question.ConditionExpression.ShouldEqual(enablementCondition);
            question.Instructions.ShouldEqual(instructions);
            question.Featured.ShouldEqual(isPreFilled);
            question.QuestionScope.ShouldEqual(scope);
            question.ValidationConditions.First().Expression.ShouldEqual(validationExpression);
            question.ValidationConditions.First().Message.ShouldEqual(validationMessage);
            question.IsTimestamp.ShouldEqual(isTimestamp);
        };

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