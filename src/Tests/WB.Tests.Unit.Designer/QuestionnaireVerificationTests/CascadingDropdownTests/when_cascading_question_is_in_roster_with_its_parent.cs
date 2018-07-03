using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_cascading_question_is_in_roster_with_its_parent: QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.FixedRoster(variable: "varRoster",
                    fixedTitles: new[] {"1", "2"},
                    children: new IComposite[]
                    {
                        new SingleQuestion
                        {
                            PublicKey = parentSingleOptionQuestionId,
                            StataExportCaption = "var",
                            QuestionType = QuestionType.SingleOption,
                            Answers = new List<Answer>
                            {
                                new Answer {AnswerText = "one", AnswerValue = "1"},
                                new Answer {AnswerText = "two", AnswerValue = "2"}
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
                                new Answer
                                {
                                    AnswerText = "child 1",
                                    AnswerValue = "1",
                                    ParentValue = "1"
                                },
                                new Answer
                                {
                                    AnswerText = "child 2",
                                    AnswerValue = "2",
                                    ParentValue = "2"
                                },
                            }
                        }
                    })
                );

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => verificationErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_not_return_WB0084_verification_error () => verificationErrors.ShouldNotContainError("WB0084");

        static QuestionnaireDocument questionnaire;
        static Guid childCascadedComboboxId;
        static Guid parentSingleOptionQuestionId;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
    }
}