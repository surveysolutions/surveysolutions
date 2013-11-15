using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_roster_size_question_that_has_roster_group_parent : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var localRosterGroupId = Guid.Parse("10000000000000000000000000000000");
            var localRosterSizeQuestionId = Guid.Parse("12333333333333333333333333333333");
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new NumericQuestion("question 1")
            {
                PublicKey = localRosterSizeQuestionId,
                IsInteger = true,
                MaxValue = 1
            });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = localRosterGroupId,
                IsRoster = true,
                RosterSizeQuestionId = localRosterSizeQuestionId,
                Children = new List<IComposite>()
                {
                    new NumericQuestion("question 2")
                    {
                        PublicKey = rosterSizeQuestionId,
                        IsInteger = true,
                        MaxValue = 1
                    }
                }
            });
            questionnaire.Children.Add(new Group() { PublicKey = rosterGroupId, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId});

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0024__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0024");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_rosterSizeQuestionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(rosterSizeQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
    }
}
