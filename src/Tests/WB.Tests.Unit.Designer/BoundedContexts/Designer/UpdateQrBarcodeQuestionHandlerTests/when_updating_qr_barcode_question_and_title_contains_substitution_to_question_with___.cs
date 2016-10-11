using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateQrBarcodeQuestionHandlerTests
{
    internal class when_updating_qr_barcode_question_and_title_contains_substitution_to_question_with_not_supported_type : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGpsQuestion(
                Guid.NewGuid(),
                chapterId,
                responsibleId,
                variableName : substitutionVariableName
            );
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
                questionnaire.UpdateQRBarcodeQuestion(
                    new UpdateQRBarcodeQuestion(
                        questionnaire.Id,
                        questionId: questionId,
                        commonQuestionParameters: new CommonQuestionParameters()
                        {
                            Title = titleWithSubstitution,
                            VariableName = "var",

                        },
                        validationExpression: null,
                        validationMessage: null,
                        responsibleId: responsibleId,
                        scope: QuestionScope.Interviewer,
                        validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>())));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title___constains__substitution__with__illegal__type__ = () =>
             new[] { "text", "contains", "substitution", "illegal", "type" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

        
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "substitution_var";
        private static string titleWithSubstitution = string.Format("title with substitution to - %{0}%", substitutionVariableName);
    }
}