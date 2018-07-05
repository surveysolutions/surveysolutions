using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_validation_message_that_references_not_existing_substitutions : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Question(questionId: questionId, validationConditions: new[]
                {
                    Create.ValidationCondition(message: "valid 0"),
                    Create.ValidationCondition(message: "valid 1"),
                    Create.ValidationCondition(message: "invalid 2 because %unknown%"),
                    Create.ValidationCondition(message: "valid 3"),
                }),
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_error_with_code_WB0017 () =>
            verificationMessages.GetError("WB0017").Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_error_WB0017_with_1_reference () =>
            verificationMessages.GetError("WB0017").References.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_error_WB0017_with_reference_with_type_Question () =>
            verificationMessages.GetError("WB0017").References.Single().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_error_WB0017_with_reference_with_id_of_question () =>
            verificationMessages.GetError("WB0017").References.Single().Id.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_return_error_WB0017_with_reference_with_validation_condition_index_equal_to_message_referencing_not_existing_variable_index () =>
            verificationMessages.GetError("WB0017").References.Single().IndexOfEntityInProperty.Should().Be(2);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionId = Guid.Parse("10000000000000000000000000000000");
    }
}