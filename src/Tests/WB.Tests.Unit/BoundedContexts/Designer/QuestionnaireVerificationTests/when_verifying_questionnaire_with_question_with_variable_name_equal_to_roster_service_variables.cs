using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_variable_name_equal_to_roster_service_variables : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            // "rowcode","rowname","rowindex","roster","Id"
            questionnaire.Children.Add(new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowcode",
                QuestionText = "hello rowcode"
            });
            questionnaire.Children.Add(new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowname",
                QuestionText = "hello rowname"
            });
            questionnaire.Children.Add(new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "rowindex",
                QuestionText = "hello rowindex"
            });
            questionnaire.Children.Add(new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "roster",
                QuestionText = "hello roster"
            });
            questionnaire.Children.Add(new TextQuestion()
            {
                PublicKey = Guid.NewGuid(),
                StataExportCaption = "Id",
                QuestionText = "hello Id"
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_5_error = () =>
            resultErrors.Count().ShouldEqual(5);

        It should_return_all_errors_with_code__WB0058 = () =>
            resultErrors.ShouldEachConformTo(e=>e.Code=="WB0058");

        It should_return_all_errors_with_1_references = () =>
            resultErrors.ShouldEachConformTo(e=>e.References.Count==1);

        It should_return_all_errors_reference_with_type_Question = () =>
            resultErrors.ShouldEachConformTo(e=>e.References.First().Type==QuestionnaireVerificationReferenceType.Question);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}
