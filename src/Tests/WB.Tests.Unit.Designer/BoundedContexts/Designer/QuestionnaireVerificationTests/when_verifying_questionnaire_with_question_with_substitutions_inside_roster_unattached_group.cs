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
    internal class when_verifying_questionnaire_with_question_with_substitutions_inside_roster_unattached_group : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            rosterSizeQuestionId = Guid.Parse("11133333333333333333333333333333");

            questionnaire = CreateQuestionnaireDocument(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    IsInteger = true,
                    StataExportCaption = "rosterSize"
                },
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new IComposite[]{ new NumericQuestion()
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

        It should_return_WB0019_error_with_2_references_on_questions = () =>
            verificationMessages.Single()
                .References.ToList()
                .ForEach(question => question.Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question));

        It should_return_WB0019_error_with_first_reference_to_question_with_substitution_text = () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(questionWithSubstitutionsId);

        It should_return_WB0019_error_with_second_reference_to_question_that_used_as_substitution_question = () =>
            verificationMessages.Single().References.ElementAt(1).Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperRosterLevelQuestionId;
        private static Guid rosterSizeQuestionId;
        private const string underDeeperRosterLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
