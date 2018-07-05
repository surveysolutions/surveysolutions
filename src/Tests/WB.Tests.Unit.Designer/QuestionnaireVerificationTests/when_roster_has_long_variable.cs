using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_roster_has_long_variable : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void should_return_WB0121_error()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.FixedRoster(Id.g1, variable: new string('a', 29)),
            });

            var verifier = CreateQuestionnaireVerifier();

            var questionnaireVerificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));


            var error = questionnaireVerificationMessages.GetError("WB0121");
            Assert.That(error, Is.Not.Null);
            Assert.That(error.Message, Does.Contain("longer than 28 characters"));
        }
    }
}