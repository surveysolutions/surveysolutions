using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Ignore("KP-6312")]
    class when_verifying_questionnaire_with_3_macros_having_same_name : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(), Create.TextQuestion(variable: "var"));
            questionnaire.Macros.Add(Guid.Parse("11111111111111111111111111111111"), Create.Macro("macroname"));
            questionnaire.Macros.Add(Guid.Parse("22222222222222222222222222222222"), Create.Macro("macroname"));
            questionnaire.Macros.Add(Guid.Parse("33333333333333333333333333333333"), Create.Macro("macroname"));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0020 = () =>
            resultErrors.ShouldEachConformTo(error => error.Code == "WB0020");

        It should_return_error_with_3_references = () =>
            resultErrors.Single().References.Count.ShouldEqual(3);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
    }
}