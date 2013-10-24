using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_question_with_substitutions_that_has_self_references : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionWithSelfSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithSelfSubstitutionsId,
                StataExportCaption = "me",
                QuestionText = "hello %me%!"
            });

            verifier = CreateQuestionnaireVerifier();
        };

        private Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        private It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        private It should_return_error_with_code__WB0016 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0016");

        private It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        private It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        private It should_return_error_reference_with_id_of_questionWithSelfSubstitutionsId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithSelfSubstitutionsId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSelfSubstitutionsId;
    }
}
