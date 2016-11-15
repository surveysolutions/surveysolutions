using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_cascading_references_makes_circular_reference : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
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
                        new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                        new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
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
                        new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                        new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" },
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
                        new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                        new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" },
                    }
                }
                );
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_WB0087_error = () => verificationErrors.First().Code.ShouldEqual("WB0087");

        It should_return_references_to_cycled_entities = () => 
            verificationErrors.SelectMany(x => x.References).ShouldEachConformTo(x =>
                new[]{parentSingleOptionQuestionId, childCascadedComboboxId, grandChildCascadingQuestion}.Contains(x.Id));

        It should_return_errors_with_references_to_questions = () => 
            verificationErrors.SelectMany(x => x.References)
                              .ShouldEachConformTo(x => x.Type == QuestionnaireVerificationReferenceType.Question);

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static Guid grandChildCascadingQuestion;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
    }
}

