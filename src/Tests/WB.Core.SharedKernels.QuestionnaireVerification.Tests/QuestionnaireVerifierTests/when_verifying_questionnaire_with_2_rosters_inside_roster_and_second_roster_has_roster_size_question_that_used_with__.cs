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
    internal class when_verifying_questionnaire_with_2_rosters_inside_roster_and_second_roster_has_roster_size_question_that_used_with_2_rosters_with_defferent_roster_levels : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            Guid rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid rosterSizeQuestionForChildRoster1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            Guid rosterSizeQuestionWithInvalidRosterLevelId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid rosterSizeQuestionWithThirdRosteLevelId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion
                {
                    PublicKey = rosterSizeQuestionId,
                    IsInteger = true,
                    MaxValue = 5
                },
                new NumericQuestion
                {
                    PublicKey = rosterSizeQuestionForChildRoster1Id,
                    IsInteger = true,
                    MaxValue = 5
                },
                new Group
                {
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group
                        {
                            IsRoster = true,
                            RosterSizeQuestionId = rosterSizeQuestionForChildRoster1Id,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion
                                {
                                    PublicKey = rosterSizeQuestionWithInvalidRosterLevelId,
                                    IsInteger = true,
                                    MaxValue = 5
                                },
                                new Group
                                {
                                    PublicKey = rosterSizeQuestionWithThirdRosteLevelId,
                                    IsRoster = true,
                                    RosterSizeQuestionId = rosterSizeQuestionWithInvalidRosterLevelId,
                                    Children = new List<IComposite>
                                    {

                                    }
                                }
                            }
                        },
                        new Group
                        {
                            PublicKey = groupWithInvalidRosterSizeQuestionId,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterSizeQuestionWithInvalidRosterLevelId,
                            Children = new List<IComposite>
                            {

                            }
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

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_rosterSizeQuestionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(groupWithInvalidRosterSizeQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid groupWithInvalidRosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
