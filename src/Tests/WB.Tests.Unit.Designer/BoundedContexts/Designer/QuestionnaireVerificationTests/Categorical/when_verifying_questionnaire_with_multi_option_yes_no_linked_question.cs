using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests.Categorical
{
    internal class when_verifying_questionnaire_with_multi_option_yes_no_linked_question : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                Create.Roster(rosterId: Guid.NewGuid(), variable:"ros",
                    children: new[] {Create.TextQuestion(questionId: linkedQuestionId, variable: "var2")}),
                Create.MultyOptionsQuestion(id: multiOptionQuestionId, variable: "var",
                    linkedToQuestionId: linkedQuestionId, yesNoView: true));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_error_with_code__WB0007__ = () =>
            resultErrors.ShouldContainError("WB0007");

        It should_return_error__WB0007__with_one_references = () =>
            resultErrors.GetError("WB0007").References.Count.ShouldEqual(1);

        It should_return_error__WB0007__reference_with_type_Question = () =>
            resultErrors.GetError("WB0007").References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error__WB0007__reference_with_id_of_multi_option_question = () =>
            resultErrors.GetError("WB0007").References.First().Id.ShouldEqual(multiOptionQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid multiOptionQuestionId = Guid.Parse("12222222222222222222222222222222");
        private static Guid linkedQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}