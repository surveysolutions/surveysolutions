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

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_question_that_has_empty_variable_name : QuestionnaireVerifierTestsContext
    {

        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("Chapter")
            {
                Children = new List<IComposite>()
                {
                    new NumericQuestion("Question with empty var")
                    {
                        PublicKey = questionWithEmptyVarId
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0057 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0057");

        It should_return_error_with_1_reference = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        private It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single()
                .References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_questionWithEmptyVar = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(questionWithEmptyVarId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;

        private static Guid questionWithEmptyVarId = Guid.Parse("11111111111111111111111111111111");
    }
}
