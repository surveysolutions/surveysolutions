using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_propagating_question_that_has_no_associated_groups : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            autopropagatedQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new AutoPropagateQuestion() { PublicKey = autopropagatedQuestionId });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0008__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0008");

        It should_return_error_with_one_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_question = () =>
            resultErrors.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_autopropagatedQuestionId = () =>
            resultErrors.Single().References.Single().Id.ShouldEqual(autopropagatedQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid autopropagatedQuestionId;
    }
}
