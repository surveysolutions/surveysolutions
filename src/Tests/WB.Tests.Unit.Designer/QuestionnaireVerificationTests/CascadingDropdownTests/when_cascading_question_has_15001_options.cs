using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_cascading_question_has_15001_options : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
            childCascadedComboboxId =      Guid.Parse("22222222222222222222222222222222");

            var childSingleOptionQuestion = new SingleQuestion
            {
                PublicKey = childCascadedComboboxId,
                QuestionType = QuestionType.SingleOption,
                StataExportCaption = "var1",
                CascadeFromQuestionId = parentSingleOptionQuestionId,
                Answers = new List<Answer>()
            };

            for (int i = 0; i <= 15000; i++)
            {
                childSingleOptionQuestion.Answers.Add(new Answer
                {
                    AnswerText = "child " + i,
                    AnswerValue = i.ToString(CultureInfo.InvariantCulture),
                    ParentValue = "1"
                });
            }

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion
            {
                PublicKey = parentSingleOptionQuestionId,
                StataExportCaption = "var",
                QuestionType = QuestionType.SingleOption,
                Answers = new List<Answer> {
                            new Answer { AnswerText = "one", AnswerValue = "1" },
                            new Answer { AnswerText = "two", AnswerValue = "2" }
                        }
                },
                childSingleOptionQuestion);

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => verificationErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0088_error () => verificationErrors.ShouldContainError("WB0088");

        [NUnit.Framework.Test] public void should_return_error_with_level_general () =>
            verificationErrors.GetError("WB0088").MessageLevel.Should().Be(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_error_with_reference_to_question () =>
            verificationErrors.GetError("WB0088").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_error_with_referece_to_question_with_error () => 
            verificationErrors.GetError("WB0088").References.First().Id.Should().Be(childCascadedComboboxId);

        static QuestionnaireDocument questionnaire;
        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
    }
}

