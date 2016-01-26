using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_macro_that_has_empty_name : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(), Create.TextQuestion(variable: "var"));
            questionnaire.Macros.Add(macroId, Create.Macro(string.Empty));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0014 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0014");

        It should_return_error_with_1_reference = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Macro = () =>
            resultErrors.Single()
                .References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Macro);

        It should_return_error_reference_with_id_of_macro = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(macroId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;

        private static readonly Guid macroId = Guid.Parse("11111111111111111111111111111111");
    }
}