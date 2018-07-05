using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_that_have_roster_level_more_than_4_and_rosters_have_different_roster_size_questions : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var rosterSizeLevel1Id = Guid.Parse("20000000000000000000000000000000");
            var rosterSizeLevel2Id = Guid.Parse("30000000000000000000000000000000");
            var rosterSizeLevel3Id = Guid.Parse("40000000000000000000000000000000");
            var rosterSizeLevel4Id = Guid.Parse("50000000000000000000000000000000");
            var rosterSizeLevel5Id = Guid.Parse("60000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.TextListQuestion(rosterSizeLevel1Id, variable: "var1", maxAnswerCount: 5),
                new Group
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeLevel1Id,
                    Children = new List<IComposite>()
                    {
                        Create.TextListQuestion(rosterSizeLevel2Id, variable: "var2", maxAnswerCount: 5),
                        new Group()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeLevel2Id,
                            Children = new List<IComposite>()
                            {
                                Create.TextListQuestion(rosterSizeLevel3Id, variable: "var3", maxAnswerCount: 5),
                                new Group()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    IsRoster = true,
                                    VariableName = "c",
                                    RosterSizeQuestionId = rosterSizeLevel3Id,
                                    Children = new List<IComposite>()
                                    {
                                        Create.TextListQuestion(rosterSizeLevel4Id, variable: "var4", maxAnswerCount: 5),
                                        new Group()
                                        {
                                            PublicKey = Guid.NewGuid(),
                                            IsRoster = true,
                                            VariableName = "d",
                                            RosterSizeQuestionId = rosterSizeLevel4Id,
                                            Children = new List<IComposite>()
                                            {
                                                Create.TextListQuestion(rosterSizeLevel5Id, variable: "var5", maxAnswerCount: 5),
                                                new Group()
                                                {
                                                    PublicKey = rosterGroupId,
                                                    IsRoster = true,
                                                    VariableName = "e",
                                                    RosterSizeQuestionId = rosterSizeLevel5Id
                                                    
                                                }
                                            }.ToReadOnlyCollection()
                                        }
                                    }.ToReadOnlyCollection()
                                }
                            }.ToReadOnlyCollection()
                        }
                    }.ToReadOnlyCollection()
                }
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_first_error_with_code__WB0055 () =>
            verificationMessages.ShouldContainError("WB0055");

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Roster () =>
            verificationMessages.GetError("WB0055").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_rosterGroupId () =>
            verificationMessages.GetError("WB0055").References.First().Id.Should().Be(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId = Guid.Parse("10000000000000000000000000000000");
    }
}
