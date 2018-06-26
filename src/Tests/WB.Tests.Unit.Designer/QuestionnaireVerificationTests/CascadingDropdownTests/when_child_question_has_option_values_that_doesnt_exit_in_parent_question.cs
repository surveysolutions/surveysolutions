using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_child_question_has_option_values_that_doesnt_exit_in_parent_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion {
                    PublicKey = parentSingleOptionQuestionId,
                    StataExportCaption = "var",
                    QuestionType = QuestionType.SingleOption,
                    Answers = new List<Answer> {
                            new Answer { AnswerText = "one", AnswerValue = "1" },
                            new Answer { AnswerText = "two", AnswerValue = "2" }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = childCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        StataExportCaption = "var1",
                        CascadeFromQuestionId = parentSingleOptionQuestionId,
                        Answers = new List<Answer> {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", ParentValue = "3" },
                            new Answer { AnswerText = "child 2", AnswerValue = "2", ParentValue = "4" },
                        }
                    }
                );
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => verificationErrors = Enumerable.ToList<QuestionnaireVerificationMessage>(verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)));

        [NUnit.Framework.Test] public void should_output_WB0084_verification_error () => verificationErrors.ShouldContainError("WB0084");

        [NUnit.Framework.Test] public void should_reference_parent_question () => 
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == parentSingleOptionQuestionId.FormatGuid());

        [NUnit.Framework.Test] public void should_reference_question () =>
            verificationErrors.GetError("WB0084").References.Should().Contain(@ref => @ref.ItemId == childCascadedComboboxId.FormatGuid());

        [NUnit.Framework.Test] public void should_return_error_with_referece_to_question () => 
            verificationErrors.GetError("WB0084").References.Should().OnlyContain(x => x.Type == QuestionnaireVerificationReferenceType.Question);

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
    }
}

