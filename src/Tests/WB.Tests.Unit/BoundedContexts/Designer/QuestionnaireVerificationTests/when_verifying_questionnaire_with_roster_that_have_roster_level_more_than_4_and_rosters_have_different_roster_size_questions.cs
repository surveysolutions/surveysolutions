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
    internal class when_verifying_questionnaire_with_roster_that_have_roster_level_more_than_4_and_rosters_have_different_roster_size_questions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeLevel1Id = Guid.Parse("20000000000000000000000000000000");
            var rosterSizeLevel2Id = Guid.Parse("30000000000000000000000000000000");
            var rosterSizeLevel3Id = Guid.Parse("40000000000000000000000000000000");
            var rosterSizeLevel4Id = Guid.Parse("50000000000000000000000000000000");
            var rosterSizeLevel5Id = Guid.Parse("60000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion() {PublicKey = rosterSizeLevel1Id, IsInteger = true, StataExportCaption = "var1"},
                new Group()
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeLevel1Id,
                    Children = new List<IComposite>()
                    {
                        new NumericQuestion() {PublicKey = rosterSizeLevel2Id, IsInteger = true, StataExportCaption = "var2"},
                        new Group()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeLevel2Id,
                            Children = new List<IComposite>()
                            {
                                new NumericQuestion() {PublicKey = rosterSizeLevel3Id, IsInteger = true, StataExportCaption = "var3"},
                                new Group()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    IsRoster = true,
                                    VariableName = "c",
                                    RosterSizeQuestionId = rosterSizeLevel3Id,
                                    Children = new List<IComposite>()
                                    {
                                        new NumericQuestion() {PublicKey = rosterSizeLevel4Id, IsInteger = true, StataExportCaption = "var4"},
                                        new Group()
                                        {
                                            PublicKey = Guid.NewGuid(),
                                            IsRoster = true,
                                            VariableName = "d",
                                            RosterSizeQuestionId = rosterSizeLevel4Id,
                                            Children = new List<IComposite>()
                                            {
                                                new NumericQuestion() {PublicKey = rosterSizeLevel5Id, IsInteger = true, StataExportCaption = "var5"},
                                                new Group()
                                                {
                                                    PublicKey = rosterGroupId,
                                                    IsRoster = true,
                                                    VariableName = "e",
                                                    RosterSizeQuestionId = rosterSizeLevel5Id,
                                                    Children = new List<IComposite>()
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                    }
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

        It should_return_first_error_with_code__WB0055 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0055");

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_rosterGroupId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId = Guid.Parse("10000000000000000000000000000000");
    }
}
