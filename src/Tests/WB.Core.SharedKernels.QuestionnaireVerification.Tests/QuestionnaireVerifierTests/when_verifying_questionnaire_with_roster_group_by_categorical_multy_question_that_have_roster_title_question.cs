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
    internal class when_verifying_questionnaire_with_roster_group_by_categorical_multy_question_that_have_roster_title_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
            rosterTitleQuestionId = Guid.Parse("11333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new MultyOptionsQuestion("question 1")
            {
                PublicKey = rosterSizeQuestionId,
                Answers = new List<IAnswer>() { new Answer() { AnswerValue = "2" } }
            });

            questionnaire.Children.Add(new Group()
            {
                PublicKey = rosterGroupId,
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestionId,
                RosterTitleQuestionId = rosterTitleQuestionId,
                Children = new List<IComposite>()
                {
                    new TextQuestion("question 1")
                    {
                        PublicKey = rosterTitleQuestionId
                    }
                }
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0036__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0036");

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
        private static Guid rosterTitleQuestionId;
    }
}
