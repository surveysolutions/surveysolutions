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
    internal class when_cascading_question_has_condition_or_validation_expression : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
                        new Answer { AnswerText = "one", AnswerValue = "1" },
                        new Answer { AnswerText = "two", AnswerValue = "2" }
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
                        new Answer { AnswerText = "child 1", ParentValue = "1", AnswerValue = "1" },
                        new Answer { AnswerText = "child 2", ParentValue = "2", AnswerValue = "2" },
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
                        new Answer { AnswerText = "child 1", AnswerValue = "1", ParentValue = "1" },
                        new Answer { AnswerText = "child 2", AnswerValue = "2", ParentValue = "2" },
                    }
                });
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => verificationErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));


        [NUnit.Framework.Test] public void should_result_contains_WB0091_error () =>
            getWB0091Error().Should().NotBeNull();

        [NUnit.Framework.Test] public void should_WB0091_error_contains_single_reference () =>
            getWB0091Error().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_WB0091_error_contains_reference_to_specified_child_cascading_question_id () =>
            getWB0091Error().References.Single().Id.Should().Be(childCascadedComboboxId);

        [NUnit.Framework.Test] public void should_result_contains_WB0092_error () =>
            getWB0092Error().Should().NotBeNull();

        [NUnit.Framework.Test] public void should_WB0092_error_contains_single_reference () =>
            getWB0092Error().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_WB0092_error_contains_reference_to_specified_child_cascading_question_id () =>
            getWB0092Error().References.Single().Id.Should().Be(secondChildId);

        private static QuestionnaireVerificationMessage getWB0091Error()
        {
            return getQuestionnaireVerificationErrorByCode("WB0091");
        }

        private static QuestionnaireVerificationMessage getWB0092Error()
        {
            return getQuestionnaireVerificationErrorByCode("WB0092");
        }

        private static QuestionnaireVerificationMessage getQuestionnaireVerificationErrorByCode(string code)
        {
            return verificationErrors.FirstOrDefault(error => error.Code == code);
        }

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static Guid secondChildId;
        static IEnumerable<QuestionnaireVerificationMessage> verificationErrors;
    }
}

