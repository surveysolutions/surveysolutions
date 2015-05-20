using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_child_question_has_option_values_that_doesnt_exit_in_parent_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion {
                    PublicKey = parentSingleOptionQuestionId,
                    StataExportCaption = "var",
                    QuestionType = QuestionType.SingleOption,
                    Answers = new List<Answer> {
                            new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                            new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = childCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        StataExportCaption = "var1",
                        CascadeFromQuestionId = parentSingleOptionQuestionId,
                        Answers = new List<Answer> {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "3" },
                            new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "4" },
                        }
                    }
                );
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationErrors = Enumerable.ToList<QuestionnaireVerificationError>(verifier.Verify(questionnaire));

        It should_output_WB0084_verification_error = () => verificationErrors.First().Code.ShouldEqual("WB0084");

        It should_reference_parent_question = () => 
            verificationErrors.First().References.ShouldContain(@ref => @ref.ItemId == parentSingleOptionQuestionId.FormatGuid());

        It should_reference_question = () =>
            verificationErrors.First().References.ShouldContain(@ref => @ref.ItemId == childCascadedComboboxId.FormatGuid());

        It should_return_error_with_referece_to_question = () => 
            verificationErrors.First().References.ShouldEachConformTo(x => x.Type == QuestionnaireVerificationReferenceType.Question);

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationError> verificationErrors;
    }
}

