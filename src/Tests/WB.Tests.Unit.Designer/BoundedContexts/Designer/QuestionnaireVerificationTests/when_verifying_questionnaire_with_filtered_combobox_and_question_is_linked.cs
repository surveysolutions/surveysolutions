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
    class when_verifying_questionnaire_with_filtered_combobox_and_question_is_linked : QuestionnaireVerifierTestsContext
    {

        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    Create.FixedRoster(variable: "varRoster",
                        fixedTitles: new[] {"fixed title", "fixed title 2"},
                        children: new IComposite[]
                        {
                            new TextQuestion()
                            {
                                PublicKey = linkedQuestionId,
                                StataExportCaption = "var2",
                                QuestionType = QuestionType.Text
                            }
                        }),

                    new SingleQuestion()
                    {
                        PublicKey = filteredComboboxId,
                        StataExportCaption = "var",
                        IsFilteredCombobox = true,
                        Answers =
                            new List<Answer>()
                            {
                                new Answer() {AnswerValue = "1", AnswerText = "text 1"},
                                new Answer() {AnswerValue = "2", AnswerText = "text 2"}
                            },
                        LinkedToQuestionId = linkedQuestionId
                    });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0074 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0074");

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
        private static Guid linkedQuestionId = Guid.Parse("20000000000000000000000000000000");
    }
}
