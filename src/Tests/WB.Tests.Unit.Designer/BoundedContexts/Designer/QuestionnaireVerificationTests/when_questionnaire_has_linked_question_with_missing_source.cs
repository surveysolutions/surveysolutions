using System;
using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_linked_question_with_missing_source : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void should_produce_WB0270_error_for_linked_to_roster_question()
        {
            Guid rosterLink = Id.g1;
            Guid questionId = Id.g2;
            
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.SingleOptionQuestion(linkedToRosterId: rosterLink, questionId: questionId)
            });

            var verifier = CreateQuestionnaireVerifier();
            var errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

            errors.ShouldContainCritical("WB0270");
        }

        [Test]
        public void should_produce_WB0270_error_for_linked_to_question_question()
        {
            Guid questionLink = Id.g1;
            Guid questionId = Id.g2;

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.SingleOptionQuestion(linkedToQuestionId: questionLink, questionId: questionId)
            });

            var verifier = CreateQuestionnaireVerifier();
            var errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

            errors.ShouldContainCritical("WB0270");
        }
    }
}