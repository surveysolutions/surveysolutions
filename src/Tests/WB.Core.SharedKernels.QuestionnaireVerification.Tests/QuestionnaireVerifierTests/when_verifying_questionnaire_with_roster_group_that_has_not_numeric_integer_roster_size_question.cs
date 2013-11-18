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
    internal class when_verifying_questionnaire_with_roster_group_that_has_not_numeric_integer_roster_size_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new TextQuestion("question 1") { PublicKey = rosterSizeQuestionId });
            questionnaire.Children.Add(new Group() { PublicKey = rosterGroupId, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId});
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0023__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0023");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_group = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_rosterGroupId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
    }
}
