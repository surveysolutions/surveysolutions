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
    internal class when_verifying_questionnaire_with_3_propagating_questions_referencing_not_existing_group_and_2_referencing_existing_propagatable_group:  QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            firstIncorrectQuestionId = Guid.Parse("1111EEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            secondIncorrectQuestionId = Guid.Parse("2222EEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            thirdIncorrectQuestionId = Guid.Parse("3333EEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            firstCorrectQuestionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            secondCorrectQuestionId = Guid.Parse("2222CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var notExistingGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var firstPropagatableGroupId = Guid.Parse("CCCC1111111111111111111111111111");
            var secondPropagatableGroupId = Guid.Parse("CCCC2222222222222222222222222222");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new AutoPropagateQuestion { PublicKey = firstIncorrectQuestionId, Triggers = new List<Guid> { notExistingGroupId } },
                new AutoPropagateQuestion { PublicKey = secondIncorrectQuestionId, Triggers = new List<Guid> { notExistingGroupId } },
                new AutoPropagateQuestion { PublicKey = thirdIncorrectQuestionId, Triggers = new List<Guid> { notExistingGroupId } },

                new AutoPropagateQuestion { PublicKey = firstCorrectQuestionId, Triggers = new List<Guid> { firstPropagatableGroupId } },
                new AutoPropagateQuestion { PublicKey = secondCorrectQuestionId, Triggers = new List<Guid> { secondPropagatableGroupId } },

                new Group { PublicKey = firstPropagatableGroupId, Propagated = Propagate.AutoPropagated },
                new Group { PublicKey = secondPropagatableGroupId, Propagated = Propagate.AutoPropagated },

                new TextQuestion { PublicKey = Guid.NewGuid() },
                new Group { PublicKey = Guid.NewGuid() }
            );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_3_errors = () =>
            resultErrors.Count().ShouldEqual(3);

        It should_return_errors_each_with_code__WB0006__ = () =>
            resultErrors.ShouldEachConformTo(error
                => error.Code == "WB0006");

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

        It should_return_error_referencing_third_incorrect_question = () =>
            resultErrors.ShouldContain(error
                => error.References.Single().Id == thirdIncorrectQuestionId);

        It should_not_return_error_referencing_first_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Single().Id == firstCorrectQuestionId);

        It should_not_return_error_referencing_second_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Single().Id == secondCorrectQuestionId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid thirdIncorrectQuestionId;
        private static Guid firstCorrectQuestionId;
        private static Guid secondCorrectQuestionId;
    }
}