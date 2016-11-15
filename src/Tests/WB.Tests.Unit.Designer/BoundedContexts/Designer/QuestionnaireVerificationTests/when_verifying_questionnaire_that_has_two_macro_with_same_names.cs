using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_that_has_two_macro_with_same_names : QuestionnaireVerifierTestsContext
    {

        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionId, Create.TextQuestion(variable: "var"));
            questionnaire.Macros.Add(macro1Id, Create.Macro("hello"));
            questionnaire.Macros.Add(macro2Id, Create.Macro("hello"));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0020 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0020");

        It should_return_message_with_1_reference = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_message_reference_with_type_Macro = () =>
            verificationMessages.Single().References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Macro);

        It should_return_message_reference_with_id_of_macro1 = () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(macro1Id);

        It should_return_message_reference_with_id_of_macro2 = () =>
            verificationMessages.Single().References.ElementAt(1).Id.ShouldEqual(macro2Id);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid macro1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macro2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");

    }
}