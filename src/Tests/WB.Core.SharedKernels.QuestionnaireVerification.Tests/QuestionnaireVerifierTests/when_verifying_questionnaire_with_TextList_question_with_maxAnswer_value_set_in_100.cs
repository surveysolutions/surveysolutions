using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_TextList_question_with_maxAnswer_value_set_in_100 : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            multiAnswerQuestionWithMaxCountId = Guid.Parse("10000000000000000000000000000000");
            textQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new TextListQuestion()
            {
                PublicKey = multiAnswerQuestionWithMaxCountId,
                StataExportCaption = "var1",
                MaxAnswerCount = 100
            });

            questionnaire.Children.Add(new TextListQuestion()
            {
                PublicKey = textQuestionId,
                StataExportCaption = "var2",
                MaxAnswerCount = null
            });
         
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0042 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0042");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_multiAnswerQuestionWithMaxCountId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(multiAnswerQuestionWithMaxCountId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multiAnswerQuestionWithMaxCountId;
        private static Guid textQuestionId;
        
    }
}