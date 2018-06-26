using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_inside_roster_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "a",
                    fixedTitles: new[] {"a", "b"},
                    children: new IComposite[]
                    {
                        new SingleQuestion()
                        {
                            PublicKey = questionId,
                            StataExportCaption = "var",
                            QuestionText = "hello %rostertitle%",
                            Answers =
                            {
                                new Answer() {AnswerValue = "1", AnswerText = "opt 1"},
                                new Answer() {AnswerValue = "2", AnswerText = "opt 2"}
                            }
                        }
                    })
                );

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_0_error () =>
            verificationMessages.Count().Should().Be(0);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionId;
    }
}
