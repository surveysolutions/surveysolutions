using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_parent_cascading_question_missing_from_questionnaire : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion
            {
                PublicKey = questionId,
                QuestionType = QuestionType.SingleOption,
                CascadeFromQuestionId = Guid.NewGuid(),
                StataExportCaption = "var",
                Answers = new List<Answer>
                {
                    new Answer { AnswerText = "one", AnswerValue = "1" },
                    new Answer { AnswerText = "two", AnswerValue = "2" }
                }
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => verificationErrors = Enumerable.ToList<QuestionnaireVerificationMessage>(verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)));

        [NUnit.Framework.Test] public void should_return_WB0086_verification_error () => verificationErrors.ShouldContainError("WB0086");

        [NUnit.Framework.Test] public void should_return_reference_to_question () => 
            verificationErrors.GetError("WB0086").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_reference_with_id_of_question () =>
            verificationErrors.GetError("WB0086").References.First().Id.Should().Be(questionId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
        static Guid questionId;
    }
}

