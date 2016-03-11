using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_multi_option_question_has_an_option_with_negative_value : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                Create.MultyOptionsQuestion(id: multiOptionQuestionId, variable: "var",
                    options: new[] {Create.Answer("1", -1), Create.Answer("2", 2)}));
            

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.CheckForErrors(questionnaire);

        It should_have_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0008__ = () =>
        resultErrors.Single().Code.ShouldEqual("WB0008");

        It should_return_message_with_one_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_multi_option_question = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(multiOptionQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid multiOptionQuestionId = Guid.Parse("12222222222222222222222222222222");
    }
}