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
    internal class when_verifying_questionnaire_with_single_option_and_options_count_more_than_200 : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            int incrementer = 0;
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("Group")
            {
                Children = new List<IComposite>
                {
                    new SingleQuestion
                    {
                        PublicKey = singleOptionId,
                        StataExportCaption = "var",
                        IsFilteredCombobox = false,
                        Answers =
                            new List<Answer>(
                                new Answer[201].Select(
                                    answer =>
                                        new Answer()
                                        {
                                            AnswerValue = incrementer.ToString(),
                                            AnswerText = (incrementer++).ToString()
                                        }))
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0075 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0076");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_questionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(singleOptionId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;

        private static Guid singleOptionId = Guid.Parse("10000000000000000000000000000000");
    }
}