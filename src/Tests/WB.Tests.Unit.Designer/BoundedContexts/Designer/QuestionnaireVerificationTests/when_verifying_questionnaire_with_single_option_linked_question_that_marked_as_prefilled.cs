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
    internal class when_verifying_questionnaire_with_single_option_linked_question_that_marked_as_prefilled : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "varRoster",
                    fixedTitles: new[] {"1", "2"},
                    children: new IComposite[]
                    {
                        new TextQuestion()
                        {
                            PublicKey = sourceLinkedQuestionId,
                            QuestionType = QuestionType.Text,
                            StataExportCaption = "var1"
                        }
                    }), new SingleQuestion()
                    {
                        PublicKey = linkedQuestionId,
                        StataExportCaption = "var2",
                        Featured = true,
                        QuestionType = QuestionType.SingleOption,
                        LinkedToQuestionId = sourceLinkedQuestionId
                    });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0090 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0090");

        It should_return_message_with_level_general = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_questionId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid sourceLinkedQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}
