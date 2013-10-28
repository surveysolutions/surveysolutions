using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects.Verification;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_propagating_question_referencing_2_not_propagatable_group_and_1_propagatable_group : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            incorrectQuestionId = Guid.Parse("11111111111111111111111111111111");

            firstNotPropagatableGroupId = Guid.Parse("EEEE1111111111111111111111111111");
            secondNotPropagatableGroupId = Guid.Parse("EEEE2222222222222222222222222222");
            propagatableGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new AutoPropagateQuestion
                {
                    PublicKey = incorrectQuestionId,
                    Triggers = new List<Guid>
                    {
                        firstNotPropagatableGroupId,
                        secondNotPropagatableGroupId,
                        propagatableGroupId
                    }
                },

                new Group { PublicKey = firstNotPropagatableGroupId, Propagated = Propagate.None },
                new Group { PublicKey = secondNotPropagatableGroupId, Propagated = Propagate.None },
                new Group { PublicKey = propagatableGroupId, Propagated = Propagate.AutoPropagated },

                new TextQuestion { PublicKey = Guid.NewGuid() },
                new Group { PublicKey = Guid.NewGuid() }
            );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0007__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0007");

        It should_return_error_having_3_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(3);

        It should_return_error_referencing_incorrect_question = () =>
            resultErrors.Single().References.ShouldContain(reference
                => reference.Type == QuestionnaireVerificationReferenceType.Question
                && reference.Id == incorrectQuestionId);

        It should_return_error_referencing_first_not_propagatable_group = () =>
            resultErrors.Single().References.ShouldContain(reference
                => reference.Type == QuestionnaireVerificationReferenceType.Group
                && reference.Id == firstNotPropagatableGroupId);

        It should_return_error_referencing_second_not_propagatable_group = () =>
            resultErrors.Single().References.ShouldContain(reference
                => reference.Type == QuestionnaireVerificationReferenceType.Group
                && reference.Id == secondNotPropagatableGroupId);

        It should_return_error_not_referencing_propagatable_group = () =>
            resultErrors.Single().References.ShouldNotContain(reference
                => reference.Type == QuestionnaireVerificationReferenceType.Group
                && reference.Id == propagatableGroupId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static Guid incorrectQuestionId;
        private static Guid firstNotPropagatableGroupId;
        private static Guid secondNotPropagatableGroupId;
        private static Guid propagatableGroupId;
    }
}