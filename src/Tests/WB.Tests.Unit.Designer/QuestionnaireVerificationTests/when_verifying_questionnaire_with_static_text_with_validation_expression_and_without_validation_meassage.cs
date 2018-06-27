using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_static_text_with_validation_expression_and_without_validation_meassage : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(),
                Create.StaticText(
                    staticTextId : staticTextId,
                    validationConditions: new List<ValidationCondition>() { new ValidationCondition(validationExpression, string.Empty)})
                );

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_return_1_message_with_code_WB0107 () =>
            verificationMessages.Count(x => x.Code == "WB0107").Should().Be(1);

        [NUnit.Framework.Test] public void should_return_messages_each_having_single_reference () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_messages_each_referencing_static_text () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.StaticText);

        [NUnit.Framework.Test] public void should_return_message_referencing_first_incorrect_question () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Single().Id.Should().Be(staticTextId);

        private static List<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid staticTextId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private const string validationExpression = "some expression";
    }
}