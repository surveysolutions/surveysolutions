using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_linked_question_reference_on_question_not_under_propagated_group : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            notUnderPropagatedGroupLinkingQuestionId = Guid.Parse("12222222222222222222222222222222");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new NumericQuestion
            {
                PublicKey = notUnderPropagatedGroupLinkingQuestionId,
                QuestionType = QuestionType.Numeric
            });

            questionnaire.Children.Add(new SingleQuestion() { PublicKey = linkedQuestionId, LinkedToQuestionId = notUnderPropagatedGroupLinkingQuestionId });
            verifier = CreateQuestionnaireVerifier();
        };

        private Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        private It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        private It should_return_error_with_code__WB0013 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0013");

        private It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        private It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        private It should_return_first_error_reference_with_id_of_linkedQuestionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(linkedQuestionId);

        private It should_return_last_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        private It should_return_last_error_reference_with_id_of_notUnderPropagatedGroupLinkingQuestionId = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(notUnderPropagatedGroupLinkingQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid linkedQuestionId;
        private static Guid notUnderPropagatedGroupLinkingQuestionId;
    }
}
