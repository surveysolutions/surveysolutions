using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_featured_categorical_multi_answers_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            featuredQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(
                new MultyOptionsQuestion()
                {
                    PublicKey = featuredQuestionId,
                    StataExportCaption = "var",
                    Answers = new List<Answer>() { new Answer() { AnswerValue = "2", AnswerText = "2" }, new Answer() { AnswerValue = "1", AnswerText = "1" } },
                    QuestionType = QuestionType.MultyOption,
                    Featured = true
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0022 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0022");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_featuredQuestionIllegalTypeId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(featuredQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid featuredQuestionId;
    }
}
