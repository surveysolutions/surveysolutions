using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_single_question_that_marked_as_prefilled : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new[]
            {
                Create.Group(groupId, children: new IComposite[]
                {
                    Create.SingleQuestion(questionId, scope: QuestionScope.Headquarter, isPrefilled: true,  optionsFilter: "(@optioncode == 100) || (@optioncode == 200)")
                })
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0029_message () =>
            verificationMessages.ShouldContainError("WB0029");

        [NUnit.Framework.Test] public void should_return_message_with_one_references () =>
            verificationMessages.GetError("WB0029").References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_one_references_with_question_type () =>
            verificationMessages.GetError("WB0029").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_with_one_references_with_id_equals_questionId () =>
            verificationMessages.GetError("WB0029").References.First().Id.Should().Be(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}