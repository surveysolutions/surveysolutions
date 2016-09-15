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

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_that_have_roster_level_more_than_4_and_rosters_have_the_same_roster_size_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new TextListQuestion() {PublicKey = rosterSizeQuestionId, MaxAnswerCount = 5, StataExportCaption = "var", QuestionType = QuestionType.TextList,},
                new Group()
                {
                    PublicKey = Guid.NewGuid(),
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        new Group()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsRoster = true,
                            VariableName = "b",
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IComposite>()
                            {
                                new Group()
                                {
                                    PublicKey = Guid.NewGuid(),
                                    IsRoster = true,
                                    VariableName = "c",
                                    RosterSizeQuestionId = rosterSizeQuestionId,
                                    Children = new List<IComposite>()
                                    {
                                        new Group()
                                        {
                                            PublicKey = Guid.NewGuid(),
                                            IsRoster = true,
                                            VariableName = "d",
                                            RosterSizeQuestionId = rosterSizeQuestionId,
                                            Children = new List<IComposite>()
                                            {
                                                new Group()
                                                {
                                                    PublicKey = rosterGroupId,
                                                    IsRoster = true,
                                                    VariableName = "e",
                                                    RosterSizeQuestionId = rosterSizeQuestionId,
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
            verificationMessages = verifier.CheckForErrors(questionnaire);

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
