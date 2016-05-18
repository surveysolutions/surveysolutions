using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_question_with_very_short_title : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.GpsCoordinateQuestion(),
                Create.TextQuestion(questionId: textQuestionId, variable: "q2", text: "nastya"));


            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_return_WB0255_warning = () => errors.ShouldContainWarning("WB0255", "Question is too short. This might be an incomplete question.");

        It should_return_error_with_references_on_text_question = () =>
          errors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_with_references_on_text_question_id = () =>
          errors.First().References.First().Id.ShouldEqual(textQuestionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        private static Guid textQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}