using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Verifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestOf(typeof(QuestionVerifications))]
    internal class QuestionVerificationsTests : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void when_categorical_multi_question_has_more_than_allowed_options_should_return_WB0075()
        {
            // arrange
            Guid filteredComboboxId = Guid.Parse("10000000000000000000000000000000");
            int incrementer = 0;
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(
                    filteredComboboxId,
                    variable: "var",
                    filteredCombobox: true,
                    options:
                    new List<Answer>(
                        new Answer[15001].Select(
                            answer =>
                                new Answer()
                                {
                                    AnswerValue = incrementer.ToString(),
                                    AnswerText = (incrementer++).ToString()
                                }))
                ));

            QuestionnaireVerifier verifier = CreateQuestionnaireVerifier();

            // act
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            // assert
            verificationMessages.Count().Should().Be(1);

            verificationMessages.Single().Code.Should().Be("WB0075");

            verificationMessages.Single().References.Count().Should().Be(1);

            verificationMessages.Single().References.First().Type.Should()
                .Be(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.Single().References.First().Id.Should().Be(filteredComboboxId);
        }

        [Test]
        public void when_verifying_categorical_multi_and_options_count_more_than_200()
        {
            Guid multiOptionId = Guid.Parse("10000000000000000000000000000000");
            int incrementer = 0;
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(
                    multiOptionId,
                    options:
                    new List<Answer>(
                        new Answer[201].Select(
                            answer =>
                                new Answer()
                                {
                                    AnswerValue = incrementer.ToString(),
                                    AnswerText = (incrementer++).ToString()
                                }))
                )
            );

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            verificationMessages.ShouldContainError("WB0076");

            verificationMessages.Single(e => e.Code == "WB0076").MessageLevel.Should()
                .Be(VerificationMessageLevel.General);

            verificationMessages.Single(e => e.Code == "WB0076").References.Count().Should().Be(1);

            verificationMessages.Single(e => e.Code == "WB0076").References.First().Type.Should()
                .Be(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.Single(e => e.Code == "WB0076").References.First().Id.Should().Be(multiOptionId);
        }
    }
}
