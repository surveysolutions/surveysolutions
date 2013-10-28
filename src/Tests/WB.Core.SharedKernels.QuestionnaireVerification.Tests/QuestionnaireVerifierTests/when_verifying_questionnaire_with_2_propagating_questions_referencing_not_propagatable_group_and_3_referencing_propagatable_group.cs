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
    internal class when_verifying_questionnaire_with_2_propagating_questions_referencing_not_propagatable_group_and_3_referencing_propagatable_group:  QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            firstIncorrectQuestionId = Guid.Parse("1111EEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            secondIncorrectQuestionId = Guid.Parse("2222EEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            firstCorrectQuestionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            secondCorrectQuestionId = Guid.Parse("2222CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            thirdCorrectQuestionId = Guid.Parse("3333CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var firstNotPropagatableGroupId = Guid.Parse("EEEE1111111111111111111111111111");
            var secondNotPropagatableGroupId = Guid.Parse("EEEE2222222222222222222222222222");
            var firstPropagatableGroupId = Guid.Parse("CCCC1111111111111111111111111111");
            var secondPropagatableGroupId = Guid.Parse("CCCC2222222222222222222222222222");
            var thirdPropagatableGroupId = Guid.Parse("CCCC3333333333333333333333333333");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new AutoPropagateQuestion { PublicKey = firstIncorrectQuestionId, Triggers = new List<Guid> { firstNotPropagatableGroupId } },
                new AutoPropagateQuestion { PublicKey = secondIncorrectQuestionId, Triggers = new List<Guid> { secondNotPropagatableGroupId } },

                new AutoPropagateQuestion { PublicKey = firstCorrectQuestionId, Triggers = new List<Guid> { firstPropagatableGroupId } },
                new AutoPropagateQuestion { PublicKey = secondCorrectQuestionId, Triggers = new List<Guid> { secondPropagatableGroupId } },
                new AutoPropagateQuestion { PublicKey = thirdCorrectQuestionId, Triggers = new List<Guid> { thirdPropagatableGroupId } },

                new Group { PublicKey = firstNotPropagatableGroupId, Propagated = Propagate.None },
                new Group { PublicKey = secondNotPropagatableGroupId, Propagated = Propagate.None },
                new Group { PublicKey = firstPropagatableGroupId, Propagated = Propagate.AutoPropagated },
                new Group { PublicKey = secondPropagatableGroupId, Propagated = Propagate.AutoPropagated },
                new Group { PublicKey = thirdPropagatableGroupId, Propagated = Propagate.AutoPropagated },

                new TextQuestion { PublicKey = Guid.NewGuid() },
                new Group { PublicKey = Guid.NewGuid() }
            );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_errors_each_with_code__WB0007__ = () =>
            resultErrors.ShouldEachConformTo(error
                => error.Code == "WB0007");

        It should_return_errors_each_having_single_reference = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Count() == 1);

        It should_return_errors_each_referencing_question = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_referencing_first_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == firstIncorrectQuestionId);

        It should_return_error_referencing_second_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == secondIncorrectQuestionId);

        It should_not_return_error_referencing_first_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Single().Id == firstCorrectQuestionId);

        It should_not_return_error_referencing_second_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Single().Id == secondCorrectQuestionId);

        It should_not_return_error_referencing_third_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Single().Id == thirdCorrectQuestionId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid firstCorrectQuestionId;
        private static Guid secondCorrectQuestionId;
        private static Guid thirdCorrectQuestionId;
    }
}