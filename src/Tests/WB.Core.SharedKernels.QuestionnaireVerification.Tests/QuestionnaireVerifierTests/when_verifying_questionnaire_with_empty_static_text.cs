using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    class when_verifying_questionnaire_with_empty_static_text : QuestionnaireVerifierTestsContext
    {

        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("Group")
            {
                Children = new List<IComposite>()
                {
                    new StaticText(entityId: staticTextId, text: string.Empty),
                    new TextQuestion() {StataExportCaption = "var"}
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0071 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0071");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_error_reference_with_id_of_questionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(staticTextId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;

        private static Guid staticTextId = Guid.Parse("10000000000000000000000000000000");
    }
}
