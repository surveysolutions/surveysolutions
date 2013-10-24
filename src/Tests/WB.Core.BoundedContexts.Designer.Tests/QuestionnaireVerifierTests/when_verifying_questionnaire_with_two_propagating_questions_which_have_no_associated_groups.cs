using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_two_propagating_questions_which_have_no_associated_groups : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            firstQuestionId = Guid.Parse("11111111111111111111111111111111");
            secondQuestionId = Guid.Parse("22222222222222222222222222222222");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new AutoPropagateQuestion { PublicKey = firstQuestionId, QuestionType = QuestionType.AutoPropagate},
                new AutoPropagateQuestion { PublicKey = secondQuestionId, QuestionType = QuestionType.AutoPropagate }
            );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_errors_each_with_code__WB0008__ = () =>
            resultErrors.ShouldEachConformTo(error =>
                error.Code == "WB0008");

        It should_return_errors_each_having_single_reference = () =>
            resultErrors.ShouldEachConformTo(error =>
                error.References.Count() == 1);

        It should_return_errors_each_referencing_question_type = () =>
            resultErrors.ShouldEachConformTo(error =>
                error.References.All(reference => reference.Type == QuestionnaireVerificationReferenceType.Question));

        It should_return_error_referencing_first_question = () =>
            resultErrors.ShouldContain(error =>
                error.References.Any(reference => reference.Id == firstQuestionId));

        It should_return_error_referencing_second_question = () =>
            resultErrors.ShouldContain(error =>
                error.References.Any(reference => reference.Id == secondQuestionId));

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid firstQuestionId;
        private static Guid secondQuestionId;
    }
}