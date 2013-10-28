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
    internal class when_verifying_questionnaire_with_2_propagating_questions_referencing_not_propagatable_group_and_1_referencing_propagatable_group:  QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            firstIncorrectQuestionId = Guid.Parse("1111EEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            secondIncorrectQuestionId = Guid.Parse("2222EEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            correctQuestionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            firstNotPropagatableGroupId = Guid.Parse("EEEE1111111111111111111111111111");
            secondNotPropagatableGroupId = Guid.Parse("EEEE2222222222222222222222222222");

            propagatableGroupId = Guid.Parse("CCCC1111111111111111111111111111");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new AutoPropagateQuestion { PublicKey = firstIncorrectQuestionId, Triggers = new List<Guid> { firstNotPropagatableGroupId } },
                new AutoPropagateQuestion { PublicKey = secondIncorrectQuestionId, Triggers = new List<Guid> { secondNotPropagatableGroupId } },
                new Group { PublicKey = firstNotPropagatableGroupId, Propagated = Propagate.None },
                new Group { PublicKey = secondNotPropagatableGroupId, Propagated = Propagate.None },

                new AutoPropagateQuestion { PublicKey = correctQuestionId, Triggers = new List<Guid> { propagatableGroupId } },
                new Group { PublicKey = propagatableGroupId, Propagated = Propagate.AutoPropagated },

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

        It should_return_errors_each_having_two_references = () =>
            resultErrors.ShouldEachConformTo(error
                => error.References.Count() == 2);

        It should_return_error_referencing_first_incorrect_question_and_first_not_propagatable_group = () =>
            resultErrors.ShouldContain(error
                => error.References.First().Type == QuestionnaireVerificationReferenceType.Question
                && error.References.First().Id == firstIncorrectQuestionId
                && error.References.Last().Type == QuestionnaireVerificationReferenceType.Group
                && error.References.Last().Id == firstNotPropagatableGroupId);

        It should_return_error_referencing_second_incorrect_question_and_second_not_propagatable_group = () =>
            resultErrors.ShouldContain(error
                => error.References.First().Type == QuestionnaireVerificationReferenceType.Question
                && error.References.First().Id == secondIncorrectQuestionId
                && error.References.Last().Type == QuestionnaireVerificationReferenceType.Group
                && error.References.Last().Id == secondNotPropagatableGroupId);

        It should_not_return_error_referencing_correct_question = () =>
            resultErrors.ShouldNotContain(error
                => error.References.First().Type == QuestionnaireVerificationReferenceType.Question
                && error.References.First().Id == correctQuestionId);

        It should_not_return_error_referencing_propagatable_group = () =>
            resultErrors.ShouldNotContain(error
                => error.References.Last().Type == QuestionnaireVerificationReferenceType.Group
                && error.References.Last().Id == propagatableGroupId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid correctQuestionId;
        private static Guid firstNotPropagatableGroupId;
        private static Guid secondNotPropagatableGroupId;
        private static Guid propagatableGroupId;
    }
}