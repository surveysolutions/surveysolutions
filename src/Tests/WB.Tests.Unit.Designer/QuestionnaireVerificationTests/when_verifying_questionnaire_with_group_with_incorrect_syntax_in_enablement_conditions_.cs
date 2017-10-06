using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_group_with_incorrect_syntax_in_enablement_conditions_ : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            const string incorrectConditionExpression = "[hehe] &=+< 5";
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Group(groupId: groupId, enablementCondition: incorrectConditionExpression)
            );
            
            verifier = CreateQuestionnaireVerifier(expressionProcessorGenerator: CreateExpressionProcessorGenerator());
            BecauseOf();
        }

        private void BecauseOf() =>
                verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_error_with_code__WB0003__ () =>
                verificationMessages.GetError("WB0003").ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_return_error_with_single_reference () =>
                verificationMessages.GetError("WB0003").References.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_error_referencing_with_type_of_group () =>
                verificationMessages.GetError("WB0003").References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        [NUnit.Framework.Test] public void should_return_error_referencing_with_specified_question_id () =>
                verificationMessages.GetError("WB0003").References.Single().Id.ShouldEqual(groupId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid groupId = Guid.Parse("22222222222222222222222222222222");
    }
}