using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    internal class when_verifying_questionnaire_with_question_that_has_substitutions_references_with_deeper_roster_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            var rosterSizeQuestionId = Guid.Parse("11133333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    IsInteger = true,
                    StataExportCaption = "var1"
                },
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new IComposite[]{
                        new NumericQuestion()
                        {
                            PublicKey = underDeeperRosterLevelQuestionId,
                            StataExportCaption = underDeeperRosterLevelQuestionVariableName
                        }}.ToReadOnlyCollection()
                },
                new SingleQuestion()
                {
                        PublicKey = questionWithSubstitutionsId,
                        StataExportCaption = "var2",
                        QuestionText = string.Format("hello %{0}%", underDeeperRosterLevelQuestionVariableName),
                        Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0019 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0019");

        It should_return_message_with_level_general = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

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

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperRosterLevelQuestionId;
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
