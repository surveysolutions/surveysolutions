using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.CascadingDropdownTests
{
    internal class when_cascading_question_has_10001_options : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");

            var childSingleOptionQuestion = new SingleQuestion
            {
                PublicKey = childCascadedComboboxId,
                QuestionType = QuestionType.SingleOption,
                StataExportCaption = "var1",
                CascadeFromQuestionId = parentSingleOptionQuestionId,
                Answers = new List<Answer>()
            };

            for (int i = 0; i <= 10000; i++)
            {
                childSingleOptionQuestion.Answers.Add(new Answer
                {
                    AnswerText = "child " + i,
                    AnswerValue = i.ToString(CultureInfo.InvariantCulture),
                    PublicKey = Guid.NewGuid(),
                    ParentValue = "1"
                });
            }

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion
            {
                PublicKey = parentSingleOptionQuestionId,
                StataExportCaption = "var",
                QuestionType = QuestionType.SingleOption,
                Answers = new List<Answer> {
                            new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                            new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                        }
                },
                childSingleOptionQuestion);

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationErrors = verifier.Verify(questionnaire);

        It should_return_WB0088_error = () => verificationErrors.First().Code.ShouldEqual("WB0088");

        static QuestionnaireDocument questionnaire;
        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationError> verificationErrors;
    }
}

