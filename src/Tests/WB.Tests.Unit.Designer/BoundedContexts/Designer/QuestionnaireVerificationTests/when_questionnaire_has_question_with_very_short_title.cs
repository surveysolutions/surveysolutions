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
    internal class when_questionnaire_has_question_with_very_short_title : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.GpsCoordinateQuestion(),
                Create.TextQuestion(questionId: textQuestionId, variable: "q2", text: "nastya"),
                Create.TextQuestion(questionId: prefilledTextQuestionId, variable: "q3", text: "nastya", scope: QuestionScope.Headquarter));


            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(questionnaire);

        It should_return_WB0255_warning = () => errors.ShouldContainWarning("WB0255", "Question is too short. This might be an incomplete question.");

        It should_return_error_with_references_on_text_question = () =>
          errors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_with_references_on_text_question_id = () =>
          errors.First().References.First().Id.ShouldEqual(textQuestionId);

        It should_not_return_warning_for_prefilled_question = () => errors.GetWarnings("WB0255").Count().ShouldEqual(1);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static readonly Guid textQuestionId = Guid.Parse("1111DDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static readonly Guid prefilledTextQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}