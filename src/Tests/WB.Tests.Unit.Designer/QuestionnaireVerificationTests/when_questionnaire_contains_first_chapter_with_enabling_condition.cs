using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class when_questionnaire_contains_first_chapter_with_enabling_condition : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void should_return_verification_error()
        {
            var groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = CreateQuestionnaireDocument(
                Create.Group(enablementCondition: "test", groupId: groupId));
            var verifier = CreateQuestionnaireVerifier();

            // act
            var errors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            // assert
            var expectedError = errors.GetError("WB0263");
            Assert.That(expectedError, Is.Not.Null, "WB0263 should be raised");
            Assert.That(expectedError.Message, Is.EqualTo(VerificationMessages.WB0263_FirstChapterHasEnablingCondition));

            Assert.That(expectedError.References.Count, Is.EqualTo(1));
            Assert.That(expectedError.References.First().Id, Is.EqualTo(groupId));
        }
    }
}