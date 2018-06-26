using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class when_questionnaire_title_has_invalid_characters : QuestionnaireVerifierTestsContext
    {
        [OneTimeSetUp] 
        public void Context () {
            var questionnaire = CreateQuestionnaireDocument();
            questionnaire.Title = "this is title [variable]";
            var verifier = CreateQuestionnaireVerifier();
            
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));
        }

        [Test] public void should_return_WB0097_message () => 
            verificationMessages.ShouldContainError("WB0097");

        [Test] public void should_return_WB0097_message_with_appropriate_message () =>
            verificationMessages.Should().Contain(x => x.Message == VerificationMessages.WB0097_QuestionnaireTitleHasInvalidCharacters);

        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }

    [TestFixture]
    internal class when_questionnaire_title_has_valid_characters : QuestionnaireVerifierTestsContext
    {
        [OneTimeSetUp] 
        public void Context () {
            var questionnaire = CreateQuestionnaireDocument();
            questionnaire.Title = "this is valid (title), \\variable/";
            var verifier = CreateQuestionnaireVerifier();
            
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));
        }

        [Test] public void should_return_WB0097_message () => 
            verificationMessages.ShouldNotContainError("WB0097");

        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}

