using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_filtered_combobox_and_options_count_more_than_15000 : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            int incrementer = 0;
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    new SingleQuestion()
                    {
                        PublicKey = filteredComboboxId,
                        StataExportCaption = "var",
                        IsFilteredCombobox = true,
                        Answers =
                            new List<Answer>(
                                new Answer[15001].Select(
                                    answer =>
                                        new Answer()
                                        {
                                            AnswerValue = incrementer.ToString(),
                                            AnswerText = (incrementer++).ToString()
                                        }))
                    });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0075 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0075");

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_questionId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(filteredComboboxId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid filteredComboboxId = Guid.Parse("10000000000000000000000000000000");
    }
}
