using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_TextList_question_that_marked_as_filled_by_supervisor : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            supervisorTextListquestionId = Guid.Parse("10000000000000000000000000000000");
            textQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
                Create.TextListQuestion(
                    supervisorTextListquestionId,
                    variable: "var1",
                    scope: QuestionScope.Supervisor
                ),
                Create.TextListQuestion(
                    variable: "var2",
                    questionId: textQuestionId
                ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0040 () =>
            verificationMessages.Single().Code.Should().Be("WB0040");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.Should().Be(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_supervisorTextListquestionId () =>
            verificationMessages.Single().References.First().Id.Should().Be(supervisorTextListquestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid supervisorTextListquestionId;
        private static Guid textQuestionId;
    }
}