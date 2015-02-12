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
    internal class when_verifying_questionnaire_with_question_that_has_substitutions_and_marked_as_featured : QuestionnaireVerifierTestsContext
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

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_error_with_code__WB0022 = () =>
            resultErrors.ShouldContain(er => er.Code == "WB0022");

        It should_return_error_with_code__WB0098 = () =>
            resultErrors.ShouldContain(er => er.Code == "WB0098");

        It should_return_errors_with_1_references = () =>
            resultErrors.ShouldEachConformTo(er => er.References.Count() == 1);

        It should_return_errors_reference_with_type_Question = () =>
            resultErrors.ShouldEachConformTo(er => er.References.First().Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_errors_reference_with_id_of_featuredQuestionIllegalTypeId = () =>
            resultErrors.ShouldEachConformTo(er => er.References.First().Id == featuredQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid featuredQuestionId;
    }
}
