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
    internal class when_cascading_references_makes_circular_reference : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            grandChildCascadingQuestion = Guid.Parse("90331351-B36E-4272-81BB-013369E27458"); 

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion
            {
                PublicKey = parentSingleOptionQuestionId,
                StataExportCaption = "var",
                QuestionType = QuestionType.SingleOption,
                CascadeFromQuestionId = grandChildCascadingQuestion,
                Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "one", AnswerValue = "1", ParentValue = "1" },
                        new Answer { AnswerText = "two", AnswerValue = "2", ParentValue = "2" }
                    }
                },
                new SingleQuestion
                {
                    PublicKey = childCascadedComboboxId,
                    QuestionType = QuestionType.SingleOption,
                    StataExportCaption = "var1",
                    CascadeFromQuestionId = parentSingleOptionQuestionId,
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "child 1", AnswerValue = "1", ParentValue = "1" },
                        new Answer { AnswerText = "child 2", AnswerValue = "2", ParentValue = "2" },
                    }
                },
                new SingleQuestion
                {
                    PublicKey = grandChildCascadingQuestion,
                    QuestionType = QuestionType.SingleOption,
                    StataExportCaption = "var3",
                    CascadeFromQuestionId = childCascadedComboboxId,
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "child 1", AnswerValue = "1", ParentValue = "1" },
                        new Answer { AnswerText = "child 2", AnswerValue = "2", ParentValue = "2" },
                    }
                }
                );
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => verificationErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0087_error () => verificationErrors.ShouldContainError("WB0087");

        [NUnit.Framework.Test] public void should_return_references_to_cycled_entities () => 
            verificationErrors.SelectMany(x => x.References).Should().Contain(x =>
                new[]{parentSingleOptionQuestionId, childCascadedComboboxId, grandChildCascadingQuestion}.Contains(x.Id));

        [NUnit.Framework.Test] public void should_return_errors_with_references_to_questions () => 
            verificationErrors.SelectMany(x => x.References)
                              .Should().Contain(x => x.Type == QuestionnaireVerificationReferenceType.Question);

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static Guid grandChildCascadingQuestion;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
    }
}

