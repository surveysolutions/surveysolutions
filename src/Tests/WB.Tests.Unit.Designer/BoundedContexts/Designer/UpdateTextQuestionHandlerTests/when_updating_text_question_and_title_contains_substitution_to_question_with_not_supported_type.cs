using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateTextQuestionHandlerTests
{
    internal class when_updating_text_question_and_title_contains_substitution_to_question_with_not_supported_type : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey : Guid.NewGuid(),
                groupPublicKey : chapterId,
                questionType : QuestionType.GpsCoordinates,
                stataExportCaption : substitutionVariableName
            ));
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
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateTextQuestion(
                    questionId: questionId,
                    title: titleWithSubstitution,
                    variableName: variableName,
                    variableLabel: null,
                    isPreFilled: isPreFilled,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    hideIfDisabled: false,
                    instructions: instructions,
                     mask: null,
                    responsibleId: responsibleId, validationCoditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(), properties: Create.QuestionProperties()));

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
        private static string variableName = "qr_barcode_question";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
    }
}