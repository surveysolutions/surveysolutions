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
    internal class when_verifying_questionnaire_with_TextList_question_with_maxAnswer_value_set_in_100 : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextListQuestion(textQuestion1Id, maxAnswerCount: 250),
                    Create.TextListQuestion(textQuestion2Id, maxAnswerCount: null)
                })
            });
           
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_error__WB0042 () =>
            verificationMessages.ShouldContainError("WB0042");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.GetError("WB0042").MessageLevel.Should().Be(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.GetError("WB0042").References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.GetError("WB0042").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_textQuestion1Id () =>
            verificationMessages.GetError("WB0042").References.First().Id.Should().Be(textQuestion1Id);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static readonly Guid textQuestion1Id = Guid.Parse("10000000000000000000000000000000");
        private static readonly Guid textQuestion2Id = Guid.Parse("20000000000000000000000000000000");
    }
}