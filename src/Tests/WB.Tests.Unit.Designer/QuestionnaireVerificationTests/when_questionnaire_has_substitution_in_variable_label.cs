using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class when_questionnaire_has_substitution_in_variable_label : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void should_return_verification_error()
        {
            var variableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = CreateQuestionnaireDocument(
                Create.Variable(id: variableId, label: "this is - %substitution%"));
            var verifier = CreateQuestionnaireVerifier();

            // act
            var errors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            // assert
            var expectedError = errors.GetError("WB0268");
            Assert.That(expectedError, Is.Not.Null, "WB0268 should be raised");
            Assert.That(expectedError.Message, Is.EqualTo(VerificationMessages.WB0268_DoesNotSupportSubstitution));

            Assert.That(expectedError.References.Count, Is.EqualTo(1));
            Assert.That(expectedError.References.First().Id, Is.EqualTo(variableId));
        }
    }
}