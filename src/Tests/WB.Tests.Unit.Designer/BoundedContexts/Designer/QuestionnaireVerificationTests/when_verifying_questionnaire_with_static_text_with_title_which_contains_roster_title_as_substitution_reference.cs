using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_static_text_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.StaticText(staticTextId: staticTextId, text: "hello %rostertitle%"),
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_error_with_code_WB0059 = () =>
            verificationMessages.GetError("WB0059").ShouldNotBeNull();

        It should_return_error_WB0059_with_1_reference = () =>
            verificationMessages.GetError("WB0059").References.Count.ShouldEqual(1);

        It should_return_error_WB0059_with_reference_with_type_StaticText = () =>
            verificationMessages.GetError("WB0059").References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_error_WB0059_with_reference_with_id_of_static_text = () =>
            verificationMessages.GetError("WB0059").References.Single().Id.ShouldEqual(staticTextId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid staticTextId = Guid.Parse("10000000000000000000000000000000");
    }
}