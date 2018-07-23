using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_chapter_has_more_than_allowed_questions : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void should_produce_error()
        {
            var childItems = Enumerable.Range(0, 401).Select(i => Create.TextQuestion(variable: "bar" + i)).ToList();
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: childItems);

            var errors = CreateQuestionnaireVerifier().CheckForErrors(Create.QuestionnaireView(questionnaire));

            errors.ShouldContainError("WB0270");
        }
    }
}