using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_that_is_used_in_title_substitutions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.Question(questionId: questionId, title: $"substitution to %{rosterVariableName}%"),
                Create.Roster(rosterId: rosterId, variable: rosterVariableName),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0018 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0018");

        It should_return_message_with_2_references = () =>
            verificationMessages.Single().References.Count.ShouldEqual(2);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_type_Roster = () =>
            verificationMessages.Single().References.Second().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_return_message_reference_with_id_of_question = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionId);

        It should_return_message_reference_with_id_of_roster = () =>
            verificationMessages.Single().References.Second().Id.ShouldEqual(rosterId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterId = Guid.Parse("10000000000000000000000000000000");
        private static string rosterVariableName = "rosterTheChosen";
        private static Guid questionId = Guid.Parse("11000000000000000000000000000000");
    }
}