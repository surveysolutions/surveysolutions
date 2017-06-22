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
    internal class when_verifying_questionnaire_with_single_option_linked_question_that_marked_as_prefilled : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "varRoster",
                    fixedTitles: new[] {"1", "2"},
                    children: new IComposite[]
                    {
                        Create.TextQuestion(
                            sourceLinkedQuestionId,
                            variable: "var1"
                        )
                    }), 
                    Create.SingleOptionQuestion(
                        linkedQuestionId,
                        variable: "var2",
                        isPrefilled: true,
                        linkedToQuestionId: sourceLinkedQuestionId
                    ));

            verifier = CreateQuestionnaireVerifier();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0090 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0090");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_questionId () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid sourceLinkedQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}
