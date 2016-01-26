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
    internal class when_verifying_questionnaire_with_linked_question_referencing_on_not_existing_question : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = linkedQuestionId,
                LinkedToQuestionId = Guid.NewGuid(),
                StataExportCaption = "var",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0011__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0011");

        It should_return_error_with_one_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_linkedQuestionId = () =>
            resultErrors.Single().References.Single().Id.ShouldEqual(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid linkedQuestionId;
    }
}
