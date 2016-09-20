using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_with_one_empty_section : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter();
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            errors = verifier.Verify(questionnaire);

        It should_not_return_WB0202_warning = () => 
            errors.GetWarning("WB0202").ShouldBeNull();

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
    }
}