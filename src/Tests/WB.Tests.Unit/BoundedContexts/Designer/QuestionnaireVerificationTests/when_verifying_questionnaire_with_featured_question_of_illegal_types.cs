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
    internal class when_verifying_questionnaire_with_featured_question_of_illegal_types : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            featuredQuestionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = featuredQuestionWithSubstitutionsId,
                StataExportCaption = "var",
                Featured = true,
                QuestionText = "hello %word%!",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0015 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0015");

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_featuredQuestionWithSubstitutionsId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(featuredQuestionWithSubstitutionsId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid featuredQuestionWithSubstitutionsId;
    }
}
