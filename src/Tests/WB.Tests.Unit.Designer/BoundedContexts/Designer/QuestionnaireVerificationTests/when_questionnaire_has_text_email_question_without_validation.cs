using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_text_email_question_without_validation : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.GpsCoordinateQuestion(),
                Create.TextQuestion(questionId: textQuestionId, variable: "q2", text: "email"));


            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_return_WB0254_warning = () => errors.ShouldContainWarning("WB0254", "Use function IsValidEmail() to validate email address.");

        It should_return_error_with_references_on_text_question = () =>
          errors.First(warning => warning.Code == "WB0254").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_with_references_on_text_question_id = () =>
          errors.First(warning => warning.Code == "WB0254").References.First().Id.ShouldEqual(textQuestionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid textQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}