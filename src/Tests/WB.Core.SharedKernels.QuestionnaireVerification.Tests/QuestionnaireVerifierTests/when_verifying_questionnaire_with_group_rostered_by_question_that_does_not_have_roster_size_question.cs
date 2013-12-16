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
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_group_rostered_by_question_that_does_not_have_roster_size_question :
        QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {

            rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    Children = new List<IComposite>() { new NumericQuestion() }
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_errors = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_first_error_with_code__WB0009 = () =>
            resultErrors.First().Code.ShouldEqual("WB0009");

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterGroupId;
    }
}
