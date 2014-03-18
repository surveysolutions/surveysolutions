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
    internal class when_verifying_questionnaire_with_roster_size_question_inside_dependent_roster : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new Group
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion
                        {
                            PublicKey = rosterSizeQuestionId,
                            IsInteger = true,
                            MaxValue = 5
                        }
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0054__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0054");

        It should_return_error_with_2_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_group = () =>
            resultErrors.Single().References.ElementAt(0).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_reference_with_id_of_rosterId = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(rosterGroupId);

        It should_return_second_error_reference_with_type_question = () =>
            resultErrors.Single().References.ElementAt(1).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_of_rosterSizeQuestionId = () =>
            resultErrors.Single().References.ElementAt(1).Id.ShouldEqual(rosterSizeQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
