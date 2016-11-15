using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.Translations
{
    internal class when_verifying_questionnaire_with_translations_and_question_that_references_absent_question_in_substitutions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                questionnaireId: questionnaireId,
                translations: new[] { Create.Translation(name: "Translation") },
                children: new IComposite[] { Create.Question(questionId: questionId, title: "title without substitution") }
            );

            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(
                questionnaireId: questionnaireId,
                translations: new[] { Create.Translation(name: "Translation") },
                children: new IComposite[] { Create.Question(questionId: questionId, title: "title with %substitution%") }
            );

            var questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);

            verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

      It should_return_message_with_code__WB0017 = () =>
            verificationMessages.ShouldContainError("WB0017");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0017").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_1_references = () =>
            verificationMessages.GetError("WB0017").References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.GetError("WB0017").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_questionWithNotExistingSubstitutionsId = () =>
            verificationMessages.GetError("WB0017").References.First().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
    }
}