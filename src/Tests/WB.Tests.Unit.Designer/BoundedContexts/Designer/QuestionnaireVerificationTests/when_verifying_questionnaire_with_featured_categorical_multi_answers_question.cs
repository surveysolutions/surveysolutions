using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_featured_categorical_multi_answers_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            featuredQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
                new MultyOptionsQuestion()
                {
                    PublicKey = featuredQuestionId,
                    StataExportCaption = "var",
                    Answers = new List<Answer>() { new Answer() { AnswerValue = "2", AnswerText = "2" }, new Answer() { AnswerValue = "1", AnswerText = "1" } },
                    QuestionType = QuestionType.MultyOption,
                    Featured = true
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_level_general = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_code__WB0022 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0022");

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_featuredQuestionIllegalTypeId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(featuredQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid featuredQuestionId;
    }
}
