using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_in_which_particular_question_and_group_have_same_id : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Group(groupId: sharedId),
                    Create.Group(groupId: Guid.Parse("22220000222255555555222200002222"), children: new IComposite[]
                    {
                        Create.Question(questionId: sharedId, variable: "var1"),
                    }),
                }),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code_WB0102 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0102");

        [NUnit.Framework.Test] public void should_return_message_with_level_critical () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.Critical);

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.Single().References.Count.ShouldEqual(2);

        [NUnit.Framework.Test] public void should_return_message_with_reference_to_group () =>
            verificationMessages.Single().References.ShouldContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Group);

        [NUnit.Framework.Test] public void should_return_message_with_reference_to_question () =>
            verificationMessages.Single().References.ShouldContain(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_with_references_having_same_shared_id () =>
            verificationMessages.Single().References.ShouldEachConformTo(reference => reference.Id == sharedId);

        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static Guid sharedId = Guid.Parse("11111111111111111111111111111111");
    }
}