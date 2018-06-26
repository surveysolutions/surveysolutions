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
    internal class when_verifying_questionnaire_with_multimedia_question_which_used_as_link_source : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "varRoster",
                    fixedTitles: new[] {"1", "2"},
                    children: new IComposite[]
                    {
                        Create.MultimediaQuestion(
                            multimediaQuestionId,
                            variable: "var1"
                        )
                    }),
                Create.SingleOptionQuestion(
                    questionId: questionWhichUsesMultimediaAsLinkSource,
                    linkedToQuestionId: multimediaQuestionId,
                    variable: "var2"
                ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0012 () =>
            verificationMessages.Single().Code.Should().Be("WB0012");

        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.Single().References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_questionWhichUsesMultimediaAsLinkSource () =>
            verificationMessages.Single().References.First().Id.Should().Be(questionWhichUsesMultimediaAsLinkSource);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_Question () =>
            verificationMessages.Single().References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_of_multimediaQuestionId () =>
            verificationMessages.Single().References.Last().Id.Should().Be(multimediaQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multimediaQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid questionWhichUsesMultimediaAsLinkSource = Guid.Parse("20000000000000000000000000000000");
    }
}
