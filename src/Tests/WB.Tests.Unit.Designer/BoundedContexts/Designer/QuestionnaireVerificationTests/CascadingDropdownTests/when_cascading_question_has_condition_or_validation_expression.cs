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

        Because of = () => verificationErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));


        It should_result_contains_WB0091_error = () =>
            getWB0091Error().ShouldNotBeNull();

        It should_WB0091_error_contains_single_reference = () =>
            getWB0091Error().References.Count().ShouldEqual(1);

        It should_WB0091_error_contains_reference_to_specified_child_cascading_question_id = () =>
            getWB0091Error().References.Single().Id.ShouldEqual(childCascadedComboboxId);

        It should_result_contains_WB0092_error = () =>
            getWB0092Error().ShouldNotBeNull();

        It should_WB0092_error_contains_single_reference = () =>
            getWB0092Error().References.Count().ShouldEqual(1);

        It should_WB0092_error_contains_reference_to_specified_child_cascading_question_id = () =>
            getWB0092Error().References.Single().Id.ShouldEqual(secondChildId);

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

