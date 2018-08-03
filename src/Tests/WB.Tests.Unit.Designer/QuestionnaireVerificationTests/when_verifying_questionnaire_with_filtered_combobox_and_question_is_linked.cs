using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_filtered_combobox_and_question_is_linked : QuestionnaireVerifierTestsContext
    {

        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    Create.FixedRoster(variable: "varRoster",
                        fixedTitles: new[] {"fixed title", "fixed title 2"},
                        children: new IComposite[]
                        {
                            new TextQuestion("text 1")
                            {
                                PublicKey = linkedQuestionId,
                                StataExportCaption = "var2",
                                QuestionType = QuestionType.Text
                            }
                        }),

                    Create.SingleQuestion(
                        filteredComboboxId,
                        variable: "var",
                        isFilteredCombobox: true,
                        options:
                            new List<Answer>()
                            {
                                new Answer() {AnswerValue = "1", AnswerText = "text 1"},
                                new Answer() {AnswerValue = "2", AnswerText = "text 2"}
                            },
                        linkedToQuestionId: linkedQuestionId
                    ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0074 () =>
            verificationMessages.Single().Code.Should().Be("WB0074");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_questionId () =>
            verificationMessages.Single().References.First().Id.Should().Be(filteredComboboxId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid filteredComboboxId = Guid.Parse("10000000000000000000000000000000");
        private static Guid linkedQuestionId = Guid.Parse("20000000000000000000000000000000");
    }
}
