using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateQrBarcodeQuestionHandlerTests
{
    internal class when_updating_qr_barcode_question_and_variable_name_not_unique : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddNumericQuestion(Guid.NewGuid(), chapterId,responsibleId, variableName:notUniqueVariableName);
            questionnaire.AddQRBarcodeQuestion(
                questionId,
                chapterId,
                responsibleId,
                title: "old title",
                variableName: "old_variable_name",
                instructions: "old instructions",
                enablementCondition: "old condition");
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateQRBarcodeQuestion(
                    new UpdateQRBarcodeQuestion(questionnaire.Id,
                    questionId, 
                    responsibleId,
                    new CommonQuestionParameters() {Title = "title", VariableName = notUniqueVariableName}, 
                    null, null, QuestionScope.Interviewer, 
                    validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>())));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__variable__should__unique__ = () =>
             new[] { "variable", "should", "unique" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

        
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string notUniqueVariableName = "var1";
    }
}