using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    [Ignore("reference validation is turned off")]
    internal class when_updating_text_question_and_condition_contains_2_id_references_and_1_of_them_invalid : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(enablementCondition) == new[] { existingQuestionId.ToString(), notExistingQuestionId.ToString() });

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, expressionProcessor: expressionProcessor);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(Create.Command.AddDefaultTypeQuestion(questionnaire.Id, existingQuestionId, "title", responsibleId, chapterId));

            questionnaire.AddQRBarcodeQuestion(
                questionId,
                chapterId,
                title: "old title",
                variableName: "old_variable_name",
                instructions: "old instructions",
                enablementCondition: "old condition",
                responsibleId: responsibleId);
        };

        Because of = () =>
             exception = Catch.Exception(() =>
                 questionnaire.UpdateTextQuestion(
                     new UpdateTextQuestion(
                         questionnaire.Id,
                         questionId,
                         responsibleId,
                         new CommonQuestionParameters() { Title = title, VariableName = variableName , EnablementCondition = enablementCondition },
                         null, scope, false,
                         new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>())));

       
        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__not__valid__expression__ = () =>
            new[] { "not", "valid", "expression" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid existingQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid notExistingQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
       private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = string.Format("[{0}] > 1 and [{1}] < 2", existingQuestionId, notExistingQuestionId);
    }
}