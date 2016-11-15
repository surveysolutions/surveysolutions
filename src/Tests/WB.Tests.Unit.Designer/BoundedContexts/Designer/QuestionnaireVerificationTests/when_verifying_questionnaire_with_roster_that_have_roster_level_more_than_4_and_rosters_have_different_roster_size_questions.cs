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
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
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
                new TextListQuestion() {PublicKey = rosterSizeLevel1Id, QuestionType = QuestionType.TextList, StataExportCaption = "var1", MaxAnswerCount = 5},
                new Group()
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeLevel1Id,
                    Children = new List<IComposite>()
                    {
                        new TextListQuestion() {PublicKey = rosterSizeLevel2Id, QuestionType = QuestionType.TextList, StataExportCaption = "var2", MaxAnswerCount = 5},
                        new Group()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeLevel2Id,
                            Children = new List<IComposite>()
                            {
                                new TextListQuestion() {PublicKey = rosterSizeLevel3Id, QuestionType = QuestionType.TextList, StataExportCaption = "var3", MaxAnswerCount = 5},
                                new Group()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    IsRoster = true,
                                    VariableName = "c",
                                    RosterSizeQuestionId = rosterSizeLevel3Id,
                                    Children = new List<IComposite>()
                                    {
                                        new TextListQuestion() {PublicKey = rosterSizeLevel4Id, QuestionType = QuestionType.TextList, StataExportCaption = "var4", MaxAnswerCount = 5},
                                        new Group()
                                        {
                                            PublicKey = Guid.NewGuid(),
                                            IsRoster = true,
                                            VariableName = "d",
                                            RosterSizeQuestionId = rosterSizeLevel4Id,
                                            Children = new List<IComposite>()
                                            {
                                                new TextListQuestion() {PublicKey = rosterSizeLevel5Id, QuestionType = QuestionType.TextList, StataExportCaption = "var5", MaxAnswerCount = 5},
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
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_first_error_with_code__WB0055 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0055");

        It should_return_message_reference_with_type_Roster = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_return_message_reference_with_id_of_rosterGroupId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId = Guid.Parse("10000000000000000000000000000000");
    }
}
