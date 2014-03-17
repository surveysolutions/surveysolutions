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
    internal class when_verifying_questionnaire_with_2_orphan_roster_groups_in_roster_and_they_must_be_treated_as_grops_with_separate_roster_levels : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {

            string questionSubstitutionsSourceFromLevel2VariableName = "i_am_from_level2";

            var parentRosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterGroupId1 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterGroupId2 = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterQuestionId1 = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var rosterQuestionId2 = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var parentRosterQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion() {PublicKey = parentRosterQuestionId, IsInteger = true, MaxValue = 3},
                new Group()
                {
                    PublicKey = parentRosterGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = parentRosterQuestionId,
                    Children = new List<IComposite>()
                    {
                        new NumericQuestion() {PublicKey = rosterQuestionId1, IsInteger = true, MaxValue = 3},
                        new NumericQuestion() {PublicKey = rosterQuestionId2, IsInteger = true, MaxValue = 3},
                        new Group()
                        {
                            PublicKey = rosterGroupId1,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterQuestionId1,
                            Children = new List<IComposite>()
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionWithSubstitutionsIdFromLevel1,
                                    QuestionText =
                                        string.Format("hello %{0}%", questionSubstitutionsSourceFromLevel2VariableName)
                                }
                            }
                        },
                        new Group()
                        {
                            PublicKey = rosterGroupId2,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterQuestionId2,
                            Children = new List<IComposite>()
                            {
                                new SingleQuestion()
                                {
                                    PublicKey = questionFromLevel2,
                                    StataExportCaption = questionSubstitutionsSourceFromLevel2VariableName
                                }
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

        It should_return_first_error_with_code__WB0019 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0019");

        It should_return_WB0019_error_with_2_references_on_questions = () =>
            resultErrors.Single()
                .References.ToList()
                .ForEach(question => question.Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question));

        It should_return_WB0019_error_with_first_reference_to_question_with_substitution_text = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(questionWithSubstitutionsIdFromLevel1);

        It should_return_WB0019_error_with_second_reference_to_question_that_used_as_substitution_question = () =>
            resultErrors.Single().References.ElementAt(1).Id.ShouldEqual(questionFromLevel2);


        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsIdFromLevel1 = Guid.Parse("12222222222222222222222222222222");
        private static Guid questionFromLevel2 = Guid.Parse("13333333333333333333333333333333");
    }
}
