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
    internal class when_verifying_questionnaire_with_question_inside_roster_that_has_substitutions_references_with_deeper_roster_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var rosterGroupId1 =       Guid.Parse("AAAAAAAAAAAAAAAA1111111111111111");
            var rosterGroupId2 =       Guid.Parse("AAAAAAAAAAAAAAAA2222222222222222");
            var rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new NumericQuestion() {PublicKey = rosterSizeQuestionId, IsInteger = true, StataExportCaption = "var1"},
                new Group()
                {
                    PublicKey = rosterGroupId1,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>()
                    {
                        new SingleQuestion()
                        {
                            PublicKey = questionWithSubstitutionsId,
                            StataExportCaption = "var2",
                            QuestionText = string.Format("hello %{0}%", underDeeperRosterLevelQuestionVariableName),
                            Answers = { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
                        },
                        new Group()
                        {
                            PublicKey = rosterGroupId2,
                            IsRoster = true,
                            VariableName = "c",
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IComposite>()
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = underDeeperRosterLevelQuestionId,
                                    StataExportCaption = underDeeperRosterLevelQuestionVariableName
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

        It should_return_message_with_code__WB0019 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0019");

        It should_return_message_with_two_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_first_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionWithSubstitutionsId);

        It should_return_last_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_message_reference_with_id_of_underDeeperPropagationLevelQuestionVariableName = () =>
            verificationMessages.Single().References.Last().Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
        private static Guid underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
