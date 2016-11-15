using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_single_option_question_has_an_option_with_decimal_value : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                Create.SingleOptionQuestion(questionId: optionQuestionId, variable: "var",
                    answerCodes: new[] {3.3m, 4m}));
            

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_have_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0114__ = () =>
        resultErrors.Single().Code.ShouldEqual("WB0114");

        It should_return_message_with_one_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_multi_option_question = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(optionQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid optionQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}