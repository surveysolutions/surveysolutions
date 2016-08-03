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
    internal class when_verifying_questionnaire_with_roster_group_by_categorical_multy_question_with_max_allowed_answers_more_then_40 : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();
            var options = new List<Answer>();
            for (int i = 0; i < 60; i++)
            {
                options.Add(Create.Option(value: i.ToString(), text: i.ToString()));
            }
            questionnaire.Children.Add(new MultyOptionsQuestion("question 1")
            {
                PublicKey = rosterSizeQuestionId,
                StataExportCaption = "var1",
                Answers = options,
                MaxAllowedAnswers = 50
            });

            questionnaire.Children.Add(new Group()
            {
                PublicKey = rosterGroupId,
                IsRoster = true,
                VariableName = "a",
                RosterSizeSource = RosterSizeSourceType.Question,
                RosterSizeQuestionId = rosterSizeQuestionId,
                Children = new List<IComposite>()
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_message_with_code__WB0100__ = () =>
            verificationMessages.ShouldContainError("WB0100");

        It should_return_message_with_1_references = () =>
            verificationMessages.GetError("WB0100").References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_question = () =>
            verificationMessages.GetError("WB0100").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_rosterSizeQuestionId = () =>
            verificationMessages.GetError("WB0100").References.First().Id.ShouldEqual(rosterSizeQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
    }
}
