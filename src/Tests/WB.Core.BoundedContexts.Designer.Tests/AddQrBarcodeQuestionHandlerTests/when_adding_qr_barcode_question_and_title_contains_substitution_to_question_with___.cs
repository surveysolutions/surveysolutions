using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.AddQrBarcodeQuestionHandlerTests
{
    internal class when_adding_qr_barcode_question_and_title_contains_substitution_to_question_with_not_supported_type : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = Guid.NewGuid(),
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.GpsCoordinates,
                StataExportCaption = substitutionVariableName
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddQRBarcodeQuestion(questionId: questionId, parentGroupId: chapterId, title: titleWithSubstitution,
                    variableName: "var",
                variableLabel: null, isMandatory: false, enablementCondition: null, instructions: null,
                    responsibleId: responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title___constains__substitution__with__illegal__type__ = () =>
             new[] { "title", "contains", "substitution", "illegal", "type" }.ShouldEachConformTo(
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