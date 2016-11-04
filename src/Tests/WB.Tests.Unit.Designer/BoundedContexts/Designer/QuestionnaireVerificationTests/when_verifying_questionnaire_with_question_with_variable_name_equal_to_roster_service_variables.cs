using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_variable_name_equal_to_roster_service_variables : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowcode",
                QuestionText = "hello rowcode"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowname",
                QuestionText = "hello rowname"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowindex",
                QuestionText = "hello rowindex"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "roster",
                QuestionText = "hello roster"
            },
                new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "Id",
                QuestionText = "hello Id"
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_5_error = () =>
            verificationMessages.Count().ShouldEqual(5);

        It should_return_all_errors_with_code__WB0058 = () =>
            verificationMessages.ShouldEachConformTo(e=>e.Code=="WB0058");

        It should_return_all_errors_with_1_references = () =>
            verificationMessages.ShouldEachConformTo(e=>e.References.Count==1);

        It should_return_all_errors_reference_with_type_Question = () =>
            verificationMessages.ShouldEachConformTo(e=>e.References.First().Type==QuestionnaireVerificationReferenceType.Question);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}
