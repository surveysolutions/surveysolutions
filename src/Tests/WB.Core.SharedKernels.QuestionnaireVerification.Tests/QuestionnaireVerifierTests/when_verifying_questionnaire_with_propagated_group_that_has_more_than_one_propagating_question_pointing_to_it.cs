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
    internal class when_verifying_questionnaire_with_propagated_group_that_has_more_than_one_propagating_question_pointing_to_it : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            autopropagatedGroupId = Guid.Parse("10000000000000000000000000000000");
            autopropagatedQuestion1Id = Guid.Parse("13333333333333333333333333333333");
            autopropagatedQuestion2Id = Guid.Parse("12222222222222222222222222222222");
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new AutoPropagateQuestion("question 1") {PublicKey = autopropagatedQuestion1Id, Triggers = new List<Guid>() { autopropagatedGroupId } });
            questionnaire.Children.Add(new AutoPropagateQuestion("question 2") {PublicKey = autopropagatedQuestion2Id, Triggers = new List<Guid>() { autopropagatedGroupId } });
            questionnaire.Children.Add(new Group() { PublicKey = autopropagatedGroupId, Propagated = Propagate.AutoPropagated });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0010__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0010");

        It should_return_error_with_3_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(3);

        It should_return_first_error_reference_with_type_group = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_reference_with_id_of_autopropagatedGroupId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(autopropagatedGroupId);

        It should_return_second_error_reference_with_type_question = () =>
            resultErrors.Single().References.Skip(1).First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_of_autopropagatedQuestion1Id = () =>
            resultErrors.Single().References.Skip(1).First().Id.ShouldEqual(autopropagatedQuestion1Id);

        It should_return_third_error_reference_with_type_question = () =>
            resultErrors.Single().References.Skip(2).First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_third_error_reference_with_id_of_autopropagatedQuestion1Id = () =>
            resultErrors.Single().References.Skip(2).First().Id.ShouldEqual(autopropagatedQuestion2Id);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid autopropagatedGroupId;
        private static Guid autopropagatedQuestion1Id;
        private static Guid autopropagatedQuestion2Id;
    }
}
