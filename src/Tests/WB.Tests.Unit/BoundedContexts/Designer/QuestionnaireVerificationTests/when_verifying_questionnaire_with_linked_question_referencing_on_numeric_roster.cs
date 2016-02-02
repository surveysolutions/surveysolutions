using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_linked_question_referencing_on_numeric_roster : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            var numericQuestionId = Guid.NewGuid();
            questionnaire.Children.Add(Create.NumericIntegerQuestion(id: numericQuestionId, variable:"num"));
            questionnaire.Children.Add(Create.Roster(rosterId: rosterId, title: "title",
                rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: numericQuestionId));
            questionnaire.Children.Add(
                new SingleQuestion()
                {
                    PublicKey = linkedQuestionId,
                    LinkedToRosterId = rosterId,
                    StataExportCaption = "var"
                });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0104__ = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0104");

        It should_return_message_with_one_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_linkedQuestionId = () =>
            verificationMessages.Single().References.Single().Id.ShouldEqual(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid linkedQuestionId;
        private static Guid rosterId = Guid.NewGuid();
    }
}