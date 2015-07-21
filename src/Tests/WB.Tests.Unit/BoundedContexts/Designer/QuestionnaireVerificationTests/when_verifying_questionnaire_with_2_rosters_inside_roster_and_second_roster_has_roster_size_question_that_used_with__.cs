using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_2_rosters_inside_roster_and_second_roster_has_roster_size_question_that_used_with_2_rosters_with_defferent_roster_levels : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            Guid rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid rosterSizeQuestionForChildRoster1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            Guid rosterSizeQuestionWithThirdRosteLevelId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion
                {
                    PublicKey = rosterSizeQuestionId,
                    StataExportCaption = "var1",
                    IsInteger = true
                },
                new NumericQuestion
                {
                    PublicKey = rosterSizeQuestionForChildRoster1Id,
                    StataExportCaption = "var2",
                    IsInteger = true
                },
                new Group
                {
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group
                        {
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeQuestionForChildRoster1Id,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion
                                {
                                    PublicKey = rosterSizeQuestionWithInvalidRosterLevelId,
                                    StataExportCaption = "var3",
                                    IsInteger = true
                                },
                                new Group
                                {
                                    PublicKey = rosterSizeQuestionWithThirdRosteLevelId,
                                    IsRoster = true,
                                    VariableName = "c",
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
                            VariableName = "d",
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

        It should_return_error_with_2_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_first_error_reference_with_type_group = () =>
            resultErrors.Single().References.ElementAt(0).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_reference_with_id_of_rosterId = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(groupWithInvalidRosterSizeQuestionId);

        It should_return_second_error_reference_with_type_question = () =>
            resultErrors.Single().References.ElementAt(1).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_reference_with_id_of_rosterSizeQuestionWithInvalidRosterLevelId = () =>
            resultErrors.Single().References.ElementAt(1).Id.ShouldEqual(rosterSizeQuestionWithInvalidRosterLevelId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid groupWithInvalidRosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterSizeQuestionWithInvalidRosterLevelId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
