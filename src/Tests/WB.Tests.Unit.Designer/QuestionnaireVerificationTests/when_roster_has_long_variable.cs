using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
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

            questionnaireVerificationMessages.ShouldContainError("WB0121");
        }
    }
}