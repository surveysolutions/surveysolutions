using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.CascadingDropdownTests
{
    internal class when_cascading_question_has_condition_or_validation_expression : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
            childCascadedComboboxId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            secondChildId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new SingleQuestion
                {
                    PublicKey = parentSingleOptionQuestionId,
                    StataExportCaption = "var",
                    QuestionType = QuestionType.SingleOption,
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                        new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                    }
                },
                new SingleQuestion
                {
                    PublicKey = childCascadedComboboxId,
                    QuestionType = QuestionType.SingleOption,
                    StataExportCaption = "var1",
                    ConditionExpression = "var == 2",
                    CascadeFromQuestionId = parentSingleOptionQuestionId,
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "child 1", ParentValue = "1", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                        new Answer { AnswerText = "child 2", ParentValue = "2", AnswerValue = "2", PublicKey = Guid.NewGuid() },
                    }
                },
                new SingleQuestion
                {
                    PublicKey = secondChildId,
                    QuestionType = QuestionType.SingleOption,
                    StataExportCaption = "var2",
                    ValidationExpression = "var2 == 2",
                    CascadeFromQuestionId = parentSingleOptionQuestionId,
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                        new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" },
                    }
                });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => verificationErrors = verifier.Verify(questionnaire);

        It should_return_WB0091_Error_for_question_with_enablement_condition = () =>
        {
            verificationErrors.ShouldContain(x => x.Code == "WB0091");
            var refereces = verificationErrors.Single(x => x.Code == "WB0091").References.ToList();
            refereces.Count.ShouldEqual(1);
            refereces.ShouldContain(x => x.Id == childCascadedComboboxId);
        };

        It should_return_WB0092_Error_for_question_with_validation_expression = () =>
        {
            verificationErrors.ShouldContain(x => x.Code == "WB0092");
            var refereces = verificationErrors.Single(x => x.Code == "WB0092").References.ToList();
            refereces.Count.ShouldEqual(1);
            refereces.ShouldContain(x => x.Id == secondChildId);
        };

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static Guid secondChildId;
        static IEnumerable<QuestionnaireVerificationError> verificationErrors;
    }
}

