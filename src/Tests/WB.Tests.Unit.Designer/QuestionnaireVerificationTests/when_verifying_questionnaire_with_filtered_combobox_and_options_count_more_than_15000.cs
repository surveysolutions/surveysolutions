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
    internal class when_verifying_questionnaire_with_filtered_combobox_and_options_count_more_than_15000 : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            int incrementer = 0;
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    Create.SingleQuestion(
                        filteredComboboxId,
                        variable: "var",
                        isFilteredCombobox: true,
                        options:
                            new List<Answer>(
                                new Answer[15001].Select(
                                    answer =>
                                        new Answer()
                                        {
                                            AnswerValue = incrementer.ToString(),
                                            AnswerText = (incrementer++).ToString()
                                        }))
                    ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0075 () =>
            verificationMessages.Single().Code.Should().Be("WB0075");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_questionId () =>
            verificationMessages.Single().References.First().Id.Should().Be(filteredComboboxId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Guid filteredComboboxId = Guid.Parse("10000000000000000000000000000000");
    }
}
